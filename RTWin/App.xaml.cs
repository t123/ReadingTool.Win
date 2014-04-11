using System;
using System.Configuration;
using System.IO;
using System.Windows;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Activation;
using NPoco;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string BaseWebAPIAddress = "http://localhost:9000/";
        public const string BaseWebSignalRAddress = "http://localhost:8888/";
        private HubConnection _hubConnection;
        private IHubProxy _mainHubProxy;
        public static User User { get; set; }
        private static IKernel _container;
        public static IKernel Container
        {
            get { return _container; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitContainer();
            InitDb();
            InitWebApi();
            InitSignalR();

            var userDialog = Container.Get<UserDialog>();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var result = userDialog.ShowDialog();
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            if (result != true)
            {
                App.User = new User(); //TODO fixme
                this.Shutdown();
            }
        }

        private void InitContainer()
        {
            _container = new StandardKernel();

            _container.Bind<Database>().ToMethod(CreateDb);
            _container.Bind<User>().ToMethod(x => App.User);

            _container.Bind<DatabaseService>().ToSelf();
            _container.Bind<UserService>().ToSelf();
            _container.Bind<LanguageService>().ToSelf();
            _container.Bind<LanguageCodeService>().ToSelf();

            _container.Bind<UserDialog>().ToSelf();
            _container.Bind<LanguageDialog>().ToSelf();
            _container.Bind<ItemDialog>().ToSelf();
            _container.Bind<ReadWindow>().ToSelf();
            _container.Bind<WatchWindow>().ToSelf();
        }

        private static Database CreateDb(IContext context)
        {
            return new Database("db");
        }

        private void InitDb()
        {
            var ds = Container.Get<DatabaseService>();
            ds.CreateAndUpgradeDatabase();
        }

        private void InitWebApi()
        {
            WebApp.Start<OWINWebAPIConfig>(BaseWebAPIAddress);
        }

        private void InitSignalR()
        {
            WebApp.Start<OWINSignalRConfig>(BaseWebSignalRAddress);
            _hubConnection = new HubConnection(App.BaseWebSignalRAddress);
            _mainHubProxy = _hubConnection.CreateHubProxy("MainHub");
            InitHub();
        }

        private WatchWindow FindWatchWindow()
        {
            Window owner = System.Windows.Application.Current.MainWindow;
            WatchWindow watchWindow = null;

            foreach (var window in owner.OwnedWindows)
            {
                if (window is WatchWindow)
                {
                    watchWindow = window as WatchWindow;
                    break;
                }
            }

            return watchWindow;
        }

        private void InitHub()
        {
            _mainHubProxy.On<string, string>("addMessage", (element, action) =>
            {
                //if (element == "mp")
                //{
                //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                //    {
                //        var readingWindow = FindWindow();

                //        if (readingWindow == null)
                //        {
                //            return;
                //        }

                //        switch (action)
                //        {
                //            case "play":
                //                readingWindow.Play();
                //                break;
                //            case "pause":
                //                bool isPlaying = readingWindow.IsPlaying();
                //                _mainHubProxy.Invoke("Send", new object[] { "mpr", isPlaying });
                //                readingWindow.Pause();
                //                break;
                //        }
                //    });
                //}
                //else 
                if (element == "video")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        double time;
                        var window = FindWatchWindow();

                        if (!double.TryParse(action, out time) || window == null)
                        {
                            _mainHubProxy.Invoke("Send", new object[] {"srtl1", -1});
                            _mainHubProxy.Invoke("Send", new object[] {"srtl2", -1});
                        }
                        else
                        {
                            var sub = window.GetSub(time);
                            _mainHubProxy.Invoke("Send", new object[] {"srtl1", sub.Item1});
                            _mainHubProxy.Invoke("Send", new object[] {"srtl2", sub.Item2});
                        }
                    });
                }
                else if (element == "modal")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => Clipboard.SetText(action, TextDataFormat.UnicodeText));
                }
            });

            _hubConnection.Start();
        }
    }
}
