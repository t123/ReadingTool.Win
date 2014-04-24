using System;
using System.IO;
using System.Linq;
using AutoMapper;
using Awesomium.Core;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Activation;
using NPoco;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Models.Dto;
using RTWin.Models.Views;
using RTWin.Services;
using RTWin.Web;

namespace RTWin
{
    public class Setup
    {
        private DatabaseService _databaseService;
        private DbSettingService _settings;

        public IKernel Container { get { return App.Container; } }

        public Setup()
        {
        }

        public static IKernel Start()
        {
            var s = new Setup();

            s.InitWebCore();
            s.CreateMappings();
            s.CheckAndUpgradeDatabase();
            s.BackupDb("Start");
            s.CleanupData();
            s.InitDb();
            s.InitWebApi();
            s.InitSignalR();

            return s.Container;
        }

        private void InitWebCore()
        {
            WebCore.Initialize(new WebConfig()
            {
                LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs"),
                RemoteDebuggingHost = "127.0.0.1",
                RemoteDebuggingPort = 1337,
            }, true);
        }

        public static void Shutdown()
        {
            var s = new Setup();
            var db = App.Container.Get<Database>();
            db.CloseSharedConnection();
            s.BackupDb("Exit");
        }

        public static IKernel InitContainer(IKernel container)
        {
            container = new StandardKernel();

            container.Bind<Database>().ToMethod(CreateDb);
            container.Bind<User>().ToMethod(x => App.User);

            container.Bind<DatabaseService>().ToSelf();
            container.Bind<DbSettingService>().ToSelf();
            container.Bind<UserService>().ToSelf();
            container.Bind<LanguageService>().ToSelf();
            container.Bind<LanguageCodeService>().ToSelf();
            container.Bind<PluginService>().ToSelf();
            container.Bind<SyncService>().ToSelf();

            container.Bind<MainWindow>().ToSelf();
            container.Bind<MainWindowControl>().ToSelf();
            container.Bind<LanguagesControl>().ToSelf();
            container.Bind<PluginsControl>().ToSelf();
            container.Bind<TextsControl>().ToSelf();
            //container.Bind<TermsControl>().ToSelf();
            container.Bind<ProfilesControl>().ToSelf();
            //container.Bind<ReadControl>().ToSelf();
            container.Bind<ItemDialog>().ToSelf();

            container.Bind<MainWindowViewModel>().ToSelf();
            container.Bind<MainWindowControlViewModel>().ToSelf();
            container.Bind<PluginsControlViewModel>().ToSelf();
            container.Bind<LanguagesControlViewModel>().ToSelf();
            container.Bind<TextsControlViewModel>().ToSelf();
            //container.Bind<TermsControlViewModel>().ToSelf();
            container.Bind<ProfilesControlViewModel>().ToSelf();
            //container.Bind<ReadControlViewModel>().ToSelf();

            return container;
        }

        private static Database CreateDb(IContext context)
        {
            return new Database("db");
        }

        private void CreateMappings()
        {
            Mapper.CreateMap<Language, LanguageModel>()
                .ForMember(x => x.SentenceRegex, y => y.MapFrom(z => z.SentenceRegex.Replace("\n", "\\n")))
                .ForMember(x => x.TermRegex, y => y.MapFrom(z => z.TermRegex.Replace("\n", "\\n")))
                .ForMember(x => x.Direction, y => y.MapFrom(z => z.Direction))
                .ForMember(x => x.Plugins, y => y.MapFrom(z => App.Container.Get<PluginService>().FindAllWithActive(z.LanguageId)))
                ;
            
            Mapper.CreateMap<User, UserModel>();
            Mapper.CreateMap<Plugin, PluginModel>();
            Mapper.CreateMap<Item, ItemModel>();
        }

        private void CheckAndUpgradeDatabase()
        {
            if (_databaseService == null)
            {
                _databaseService = Container.Get<DatabaseService>();
            }

            _databaseService.CreateAndUpgradeDatabase();
        }

        public void BackupDb(string identifier)
        {
            if (_settings == null)
            {
                _settings = Container.Get<DbSettingService>();
            }

            var backupDatabase = _settings.Get<bool?>(DbSetting.Keys.BackupDatabase) ?? true;

            if (!backupDatabase)
            {
                return;
            }

            var databaseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "rtwin.sqlite");

            if (!File.Exists(databaseFile))
            {
                return;
            }

            var backupPath = _settings.Get<string>(DbSetting.Keys.BackupDatabasePath);
            var maxBackups = _settings.Get<int?>(DbSetting.Keys.BackupMax) ?? 16;

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
            WebApp.Start<OWINWebAPIConfig>(_settings.Get<string>(DbSetting.Keys.BaseWebAPIAddress));
        }

        private void InitSignalR()
        {
            WebApp.Start<OWINSignalRConfig>(_settings.Get<string>(DbSetting.Keys.BaseWebSignalRAddress));
            MainHub.Init();
        }
    }
}
