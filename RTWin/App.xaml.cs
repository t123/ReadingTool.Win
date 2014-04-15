using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Awesomium.Core;
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
        private HubConnection _hubConnection;
        private IHubProxy _mainHubProxy;
        public static User User { get; set; }
        private static IKernel _container;
        private DatabaseService _databaseService;

        public static IKernel Container
        {
            get { return _container; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitContainer();

            _databaseService = Container.Get<DatabaseService>();

            InitDb();
            InitWebApi();
            InitSignalR();

            var skip = new Ninject.Parameters.ConstructorArgument("skip", true);
            var userDialog = Container.Get<UserDialog>(skip);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var result = userDialog.ShowDialog();
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            if (result != true)
            {
                App.User = new User(); //TODO fixme
                this.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            WebCore.Shutdown();
            base.OnExit(e);
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
            _container.Bind<PluginService>().ToSelf();

            _container.Bind<UserDialog>().ToSelf();
            _container.Bind<LanguageDialog>().ToSelf();
            _container.Bind<ItemDialog>().ToSelf();
            _container.Bind<ReadWindow>().ToSelf();
            _container.Bind<WatchWindow>().ToSelf();
            _container.Bind<PluginDialog>().ToSelf();
        }

        private static Database CreateDb(IContext context)
        {
            return new Database("db");
        }

        private void InitDb()
        {
            _databaseService.CreateAndUpgradeDatabase();
        }

        private void InitWebApi()
        {
            WebApp.Start<OWINWebAPIConfig>(_databaseService.GetSetting<string>(DbSetting.Keys.BaseWebAPIAddress));
        }

        private void InitSignalR()
        {
            WebApp.Start<OWINSignalRConfig>(_databaseService.GetSetting<string>(DbSetting.Keys.BaseWebSignalRAddress));
            _hubConnection = new HubConnection(_databaseService.GetSetting<string>(DbSetting.Keys.BaseWebSignalRAddress));
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
                if (element == "video")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        double time;
                        var window = FindWatchWindow();

                        if (!double.TryParse(action, out time) || window == null)
                        {
                            _mainHubProxy.Invoke("Send", new object[] { "srtl1", -1 });
                            _mainHubProxy.Invoke("Send", new object[] { "srtl2", -1 });
                        }
                        else
                        {
                            var sub = window.GetSub(time);
                            _mainHubProxy.Invoke("Send", new object[] { "srtl1", sub.Item1 });
                            _mainHubProxy.Invoke("Send", new object[] { "srtl2", sub.Item2 });
                        }
                    });
                }
                else if (element == "modal")
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(action, TextDataFormat.UnicodeText), DispatcherPriority.Background);
                }
            });

            _hubConnection.Start();
        }

    }
}
