using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AutoMapper;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Activation;
using NPoco;
using NPoco.FluentMappings;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Models;
using RTWin.Models.Views;
using RTWin.Services;
using RTWin.Web;

namespace RTWin
{
    public class Setup
    {
        private IKernel _container;
        private DatabaseService _databaseService;
        private HubConnection _hubConnection;
        private IHubProxy _mainHubProxy;

        public IKernel Container { get { return _container; } }

        public Setup(IKernel container)
        {
            _container = container;
        }

        public static IKernel Start(IKernel container)
        {
            var s = new Setup(container);
            s.InitContainer();
            s.CreateMappings();
            s.BackupDb("Start");
            s.CleanupData();
            s.InitDb();
            s.InitWebApi();
            s.InitSignalR();
            return s.Container;
        }

        public static void Shutdown(IKernel container)
        {
            var s = new Setup(container);
            var db = App.Container.Get<Database>();
            db.CloseSharedConnection();
            s.BackupDb("Exit");
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

            _container.Bind<MainWindow>().ToSelf();
            _container.Bind<MainWindowControl>().ToSelf();
            _container.Bind<LanguagesControl>().ToSelf();
            _container.Bind<PluginsControl>().ToSelf();
            _container.Bind<TextsControl>().ToSelf();
            _container.Bind<TermsControl>().ToSelf();
            _container.Bind<ProfilesControl>().ToSelf();
            _container.Bind<ReadControl>().ToSelf();

            _container.Bind<UserDialog>().ToSelf();
            _container.Bind<PromptDialog>().ToSelf();
            _container.Bind<ItemDialog>().ToSelf();

            _container.Bind<MainWindowViewModel>().ToSelf();
            _container.Bind<PluginsControlViewModel>().ToSelf();
            _container.Bind<LanguagesControlViewModel>().ToSelf();
            _container.Bind<TermsControlViewModel>().ToSelf();
            _container.Bind<ProfilesControlViewModel>().ToSelf();
            _container.Bind<ReadControlViewModel>().ToSelf();

            _databaseService = Container.Get<DatabaseService>();
        }

        private static Database CreateDb(IContext context)
        {
            return new Database("db");
        }

        private void CreateMappings()
        {
            Mapper.CreateMap<Language, LanguageModel>()
                .ForMember(x => x.SentenceRegex, y => y.MapFrom(z => z.Settings.SentenceRegex.Replace("\n", "\\n")))
                .ForMember(x => x.TermRegex, y => y.MapFrom(z => z.Settings.TermRegex.Replace("\n", "\\n")))
                .ForMember(x => x.Direction, y => y.MapFrom(z => z.Settings.Direction))
                .ForMember(x => x.Plugins, y => y.MapFrom(z => App.Container.Get<PluginService>().FindAllWithActive(z.LanguageId)))
                ;
        }

        public void BackupDb(string identifier)
        {
            if (_databaseService == null)
            {
                _databaseService = _container.Get<DatabaseService>();
            }

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

        private void CleanupData()
        {
            var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var directory = new DirectoryInfo(dataPath);
            var files = directory.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(x => x.LastWriteTime < DateTime.Now.AddDays(-2));

            foreach (var file in files.Where(x => x.Extension == ".html" || x.Extension == ".xml"))
            {
                file.Delete();
            }

            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs");

            if (!Directory.Exists(logPath))
            {
                return;
            }

            directory = new DirectoryInfo(dataPath);
            files = directory.GetFiles("*.log", SearchOption.TopDirectoryOnly).Where(x => x.LastWriteTime < DateTime.Now.AddDays(-7));

            foreach (var file in files)
            {
                file.Delete();
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

                        if (!double.TryParse(action, out time) || owner.MainWindowViewModel == null)
                        {
                            _mainHubProxy.Invoke("Send", new object[] { "srtl1", -1 });
                            _mainHubProxy.Invoke("Send", new object[] { "srtl2", -1 });
                        }
                        else
                        {
                            var sub = owner.MainWindowViewModel.GetSub(time);
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
