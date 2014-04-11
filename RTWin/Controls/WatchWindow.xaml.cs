using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Awesomium.Core;
using Awesomium.Core.Data;
using Microsoft.AspNet.SignalR.Client;
using Ninject;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Services;
using MediaState = System.Windows.Controls.MediaState;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for WatchWindow.xaml
    /// </summary>
    public partial class WatchWindow : Window, IReadingWindow
    {
        private Item _item;
        private ParserOutput _output;
        private System.Timers.Timer _timer;
        private HubConnection hubConnection;
        private IHubProxy mainHubProxy;
        private int _lastL1 = -1;
        private int _lastL2 = -1;
        private bool canPlay = false;
        private TermService _termService;
        private LanguageService _languageService;

        public WatchWindow(Item item)
        {
            _item = item;
            _languageService = App.Container.Get<LanguageService>();
            _termService = App.Container.Get<TermService>();

            InitializeComponent();

            BindItem();
        }

        private void BindItem()
        {
            ParserInput pi = new ParserInput()
                .WithItem(_item)
                .IsParallel()
                .WithLanguage1(_languageService.FindOne(_item.L1LanguageId))
                .WithLanguage2(_languageService.FindOne(_item.L2LanguageId))
                .WithTerms(_termService.FindAllByLanguage(_item.L1LanguageId))
                .WithHtml(HtmlLoader.Instance.Html)
                ;

            var ps = new VideoParserService(pi);
            _output = ps.Parse();

            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "awesomium"
                );

            var session = WebCore.Sessions.FirstOrDefault(x => x.DataPath == path);

            if (session == null)
            {
                session = WebCore.CreateWebSession(path, new WebPreferences()
                {
                    SmoothScrolling = true,
                }
                    );

                session.AddDataSource("rtad", new DirectoryDataSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data")));
                session.AddDataSource("rtdb", new DirectoryDataSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")));
            }

            WebControl.WebSession = session;
            WebControl.ProcessCreated += WebControl_ProcessCreated;

            if (!string.IsNullOrWhiteSpace(_item.MediaUri) && File.Exists(_item.MediaUri))
            {
                mePlayer.LoadedBehavior = MediaState.Manual;
                mePlayer.Source = new Uri(_item.MediaUri, UriKind.Absolute);
                mePlayer.ScrubbingEnabled = true;
                mePlayer.Loaded += mePlayer_Loaded;
                mePlayer.MediaOpened += mePlayer_MediaOpened;
                mePlayer.MediaFailed += mePlayer_MediaFailed;

                _timer = new System.Timers.Timer();
                _timer.Interval = 50;
                _timer.Elapsed += (sender, args) => Application.Current.Dispatcher.Invoke(() =>
                {
                    var state = GetMediaState(mePlayer);

                    if (state != MediaState.Play)
                    {
                        return;
                    }

                    var position = mePlayer.Position.TotalSeconds;
                    var l1Sub = ps.L1Srt.FirstOrDefault(x => x.Start < position && x.End > position);
                    var l2Sub = ps.L2Srt.FirstOrDefault(x => x.Start < position && x.End > position);

                    if (l1Sub != null)
                    {
                        if (l1Sub.LineNo != _lastL1)
                        {
                            mainHubProxy.Invoke("Send", new object[] { "srtl1", l1Sub.LineNo });
                            _lastL1 = l1Sub.LineNo;
                        }
                    }
                    else
                    {
                        if (_lastL1 != -1)
                        {
                            mainHubProxy.Invoke("Send", new object[] { "srtl1", "-1" });
                            _lastL1 = -1;
                        }
                    }

                    if (l2Sub != null)
                    {
                        if (l2Sub.LineNo != _lastL2)
                        {
                            mainHubProxy.Invoke("Send", new object[] { "srtl2", l2Sub.LineNo });
                            _lastL2 = l2Sub.LineNo;
                        }
                    }
                    else
                    {
                        if (_lastL2 != -1)
                        {
                            mainHubProxy.Invoke("Send", new object[] { "srtl2", "-1" });
                            _lastL2 = -1;
                        }
                    }
                });

                hubConnection = new HubConnection(App.BaseWebSignalRAddress);
                mainHubProxy = hubConnection.CreateHubProxy("MainHub");
                hubConnection.Start();
            }

            Title = (_item.CollectionNo == null ? "" : (_item.CollectionNo.ToString() + ". ")) +
                    (string.IsNullOrWhiteSpace(_item.CollectionName) ? "" : (_item.CollectionName + " - ")) +
                    _item.L1Title
                    ;
        }

        void WebControl_ProcessCreated(object sender, WebViewEventArgs e)
        {
            canPlay = true;
        }

        void mePlayer_Loaded(object sender, RoutedEventArgs e)
        {
            mePlayer.Play();
            mePlayer.Pause();
            _timer.Start();
        }

        void mePlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            throw e.ErrorException;
        }

        private void mePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (!canPlay)
                return;

            var state = GetMediaState(mePlayer);

            if (state == MediaState.Play)
            {
                if (mePlayer.CanPause)
                {
                    mePlayer.Pause();
                    btnPlayPause.Content = "Play";
                }
            }
            else
            {
                mePlayer.Play();
                btnPlayPause.Content = "Pause";
            }
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            SliderPosition.Value = 0;
            mePlayer.Stop();
            btnPlayPause.Content = "Play";
            _lastL1 = _lastL2 = -1;
        }

        private void SliderPosition_OnValueChangedliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!canPlay)
                return;

            var state = GetMediaState(mePlayer);

            mePlayer.Pause();
            mePlayer.Position = TimeSpan.FromSeconds(SliderPosition.Value);
            mePlayer.Play();
            _lastL1 = _lastL2 = -1;

            if (state == MediaState.Pause)
            {
                mePlayer.Pause();
            }
        }

        public void Pause()
        {
            if (mePlayer.CanPause)
            {
                mePlayer.Pause();
                btnPlayPause.Content = "Play";
            }
        }

        public void Play()
        {
            mePlayer.Position = TimeSpan.FromSeconds(mePlayer.Position.TotalSeconds - 1.5);
            mePlayer.Play();
            btnPlayPause.Content = "Pause";
        }

        public bool IsPlaying()
        {
            return GetMediaState(mePlayer) == MediaState.Play;
        }

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }

        private void WebControl_OnNativeViewInitialized(object sender, WebViewEventArgs e)
        {
            while (!WebCore.IsInitialized)
            {
                Thread.Sleep(500);
            }

            if (WebCore.IsInitialized && _item != null)
            {
                WebControl.Source = ("asset://rtdb/" + _item.ItemId + ".html").ToUri();
            }
        }
    }
}
