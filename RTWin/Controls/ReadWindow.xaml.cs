using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for ReadWindow.xaml
    /// </summary>
    public partial class ReadWindow : Window, IReadingWindow
    {
        protected Item _item;
        private ParserOutput _output;
        private System.Timers.Timer _timer;
        private LanguageService _languageService;
        private TermService _termService;

        public ReadWindow(Item item)
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
                .IsParallel(false)
                .WithLanguage1(_languageService.FindOne(_item.L1LanguageId))
                .WithLanguage2(_languageService.FindOne(_item.L2LanguageId))
                .WithTerms(_termService.FindAllByLanguage(_item.L1LanguageId))
                .WithHtml(HtmlLoader.Instance.Html)
                ;

            var ps = new ParserService(pi);
            _output = ps.Parse();

            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "awesomium"
                );

            //if (!WebCore.IsInitialized)
            //{
            //    var config = new WebConfig();
            //    config.PluginsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            //    config.LogLevel = LogLevel.Verbose;
            //    config.LogPath = @"C:\gitrepository\RTWin\RTWin\bin\Debug\plugins\log.txt";
            //    WebCore.Initialize(config, true);
            //}

            var session = WebCore.Sessions.FirstOrDefault(x => x.DataPath == path);

            if (session == null)
            {
                session = WebCore.CreateWebSession(path, new WebPreferences()
                        {
                            SmoothScrolling = true,
                            FileAccessFromFileURL = true,
                            UniversalAccessFromFileURL = true,
                            DefaultEncoding = "utf-8",
                            AllowInsecureContent = true,
                            Plugins = true,
                        }
                    );

                //session.AddDataSource("rtad", new DirectoryDataSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data")));
                //session.AddDataSource("rtdb", new DirectoryDataSource(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")));
            }

            WebControl.WebSession = session;
            WebControl.ConsoleMessage += WebControl_ConsoleMessage;

            if (!string.IsNullOrWhiteSpace(_item.MediaUri) && File.Exists(_item.MediaUri))
            {
                mePlayer.LoadedBehavior = MediaState.Manual;
                mePlayer.Source = _item.MediaUri.ToUri();
                mePlayer.MediaOpened += mePlayer_MediaOpened;

                _timer = new System.Timers.Timer();
                _timer.Interval = 50;
                _timer.Elapsed += (sender, args) => Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTime.Text = string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                        mePlayer.Position.Hours,
                        mePlayer.Position.Minutes,
                        mePlayer.Position.Seconds,
                        mePlayer.Position.Milliseconds);
                });
            }
            else
            {
                spMediaPlayer.Visibility = Visibility.Hidden;
            }

            Title = (_item.CollectionNo == null ? "" : (_item.CollectionNo.ToString() + ". ")) +
                    (string.IsNullOrWhiteSpace(_item.CollectionName) ? "" : (_item.CollectionName + " - ")) +
                    _item.L1Title
                    ;
        }

        void WebControl_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            return;
        }

        void mePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SliderPosition.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
            _timer.Start();
        }

        private void WebControl_OnNativeViewInitialized(object sender, WebViewEventArgs e)
        {
            while (!WebCore.IsInitialized)
            {
                Thread.Sleep(500);
            }

            if (WebCore.IsInitialized && _item != null)
            {
                //WebControl.Source = ("asset://rtdb/" + _item.ItemId + ".html").ToUri();
                WebControl.Source = ("http://localhost:9000/api/item/" + _item.ItemId).ToUri();
            }
        }

        private void SliderPosition_OnValueChangedliderPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var state = GetMediaState(mePlayer);

            mePlayer.Pause();
            mePlayer.Position = TimeSpan.FromSeconds(SliderPosition.Value);
            mePlayer.Play();

            if (state == MediaState.Pause)
            {
                mePlayer.Pause();
            }
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
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

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            SliderPosition.Value = 0;
            mePlayer.Stop();
            btnPlayPause.Content = "Play";
        }

        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }
    }
}
