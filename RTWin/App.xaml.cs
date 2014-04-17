using System;
using System.Configuration;
using System.IO;
using System.Linq;
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

            BackupDb("Start");

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
            var db = App.Container.Get<Database>();
            db.CloseSharedConnection();

            BackupDb("Exit");

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
            _container.Bind<PluginDialog>().ToSelf();
        }

        private static Database CreateDb(IContext context)
        {
            return new Database("db");
        }

        private void BackupDb(string identifier)
        {
            var backupDatabase = _databaseService.GetSetting<bool?>(DbSetting.Keys.BackupDatabase) ?? true;

            if (!backupDatabase)
            {
                return;
            }

            var databaseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "rtwin.sqlite");

            if (!File.Exists(databaseFile))
            {
                return;
            }

            var backupPath = _databaseService.GetSetting<string>(DbSetting.Keys.BackupDatabasePath);
            var maxBackups = _databaseService.GetSetting<int?>(DbSetting.Keys.BackupMax) ?? 16;

            if (string.IsNullOrWhiteSpace(backupPath))
            {
                backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Backup");
            }

            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            string backupFile;
            int counter = 1;
            do
            {
                string filename = string.Format("{0:yyyyMMdd}-{1}-{2}.sqlite", DateTime.Now, identifier, counter);
                backupFile = Path.Combine(backupPath, filename);
                counter++;
            } while (File.Exists(backupFile));

            File.Copy(databaseFile, backupFile, false);

            var directory = new DirectoryInfo(backupPath);

            var difference = directory.GetFiles("*.sqlite").Length - maxBackups;
            if (difference > 0)
            {
                var files = directory.GetFiles("*.sqlite").OrderBy(x => x.CreationTime).Take(difference);

                foreach (var file in files)
                {
                    File.Delete(file.FullName);
                }
            }
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

        private void InitHub()
        {
            _mainHubProxy.On<string, string>("addMessage", (element, action) =>
            {
                if (element == "video")
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        double time;
                        MainWindow owner = System.Windows.Application.Current.MainWindow as MainWindow;

                        if (owner == null)
                        {
                            return;
                        }

                        if (!double.TryParse(action, out time) || owner.WatchControl == null)
                        {
                            _mainHubProxy.Invoke("Send", new object[] { "srtl1", -1 });
                            _mainHubProxy.Invoke("Send", new object[] { "srtl2", -1 });
                        }
                        else
                        {
                            var sub = owner.WatchControl.GetSub(time);
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
