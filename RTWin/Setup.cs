using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using AutoMapper;
using Awesomium.Core;
using log4net;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Activation;
using NPoco;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Models.Dto;
using RTWin.Models.Views;
using RTWin.Properties;
using RTWin.Services;
using RTWin.Web;

namespace RTWin
{
    public class Setup
    {
        private DatabaseService _databaseService;
        private DbSettingService _settings;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class Error
        {
            public string Message { get; set; }
            public Exception Exception { get; set; }

            public Error(string message)
                : this(message, null)
            {
            }

            public Error(string message, Exception exception)
            {
                Message = message;
                Exception = exception;
            }
        }

        public IKernel Container { get { return App.Container; } }

        private Dictionary<string, Error> _errors;

        public IReadOnlyDictionary<string, Error> Errors
        {
            get { return new ReadOnlyDictionary<string, Error>(_errors); }
        }

        public bool HasErrors
        {
            get { return _errors.Any(); }
        }

        public void AddError(string key, Error error)
        {
            _errors[key] = error;
        }

        private static Setup _s;
        public static Setup Instance
        {
            get { return _s; }
        }

        public Setup()
        {
            _errors = new Dictionary<string, Error>();
        }

        public static IKernel Start()
        {
            _s = new Setup();

            _s.InitWebCore();
            _s.CreateMappings();
            _s.CheckAndUpgradeDatabase();
            _s.BackupDb("Start");
            _s.CleanupData();
            _s.InitDb();
            _s.InitWebApi();
            _s.InitSignalR();

            return _s.Container;
        }

        private void InitWebCore()
        {
            try
            {
                WebCore.Initialize(new WebConfig()
                {
                    LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs"),
                    RemoteDebuggingHost = "127.0.0.1",
                    RemoteDebuggingPort = 1337,
                }, true);
            }
            catch (Exception exception)
            {
                _errors["InitWebCore"] = new Error("Could not initialize WebCore", exception);
                Log.Error(exception);
            }
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
            container.Bind<TermsControl>().ToSelf();
            container.Bind<ProfilesControl>().ToSelf();
            container.Bind<ReadControl>().ToSelf();
            container.Bind<ItemDialog>().ToSelf();
            container.Bind<SettingsControl>().ToSelf();

            container.Bind<MainWindowViewModel>().ToSelf();
            container.Bind<MainWindowControlViewModel>().ToSelf();
            container.Bind<PluginsControlViewModel>().ToSelf();
            container.Bind<LanguagesControlViewModel>().ToSelf();
            container.Bind<TextsControlViewModel>().ToSelf();
            container.Bind<TermsControlViewModel>().ToSelf();
            container.Bind<ProfilesControlViewModel>().ToSelf();
            container.Bind<ReadControlViewModel>().ToSelf();
            container.Bind<SettingsControlViewModel>().ToSelf();

            return container;
        }

        private static Database CreateDb(IContext context)
        {
            return new Database("db");
        }

        private void CreateMappings()
        {
            try
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
                Mapper.CreateMap<Term, TermModel>();
                Mapper.CreateMap<DbSetting, DbSettingModel>();
            }
            catch (Exception exception)
            {
                _errors["CreateMappings"] = new Error("Could not create mappings", exception);
                Log.Error(exception);
            }
        }

        private void CheckAndUpgradeDatabase()
        {
            try
            {
                if (_databaseService == null)
                {
                    _databaseService = Container.Get<DatabaseService>();
                }

                _databaseService.CreateAndUpgradeDatabase();
            }
            catch (Exception exception)
            {
                _errors["CheckAndUpgradeDatabase"] = new Error("Could not create/upgrade database", exception);
                Log.Error(exception);
            }
        }

        public void BackupDb(string identifier)
        {
            try
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
                        try
                        {
                            File.Delete(file.FullName);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _errors["BackupDb"] = new Error("Could not backup database", exception);
                Log.Error(exception);
            }
        }

        private void CleanupData()
        {
            try
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
            catch (Exception exception)
            {
                _errors["CleanupData"] = new Error("Could not remove temporary data", exception);
                Log.Error(exception);
            }
        }

        private void InitDb()
        {
            try
            {
                _databaseService.CreateAndUpgradeDatabase();
            }
            catch (Exception exception)
            {
                _errors["InitDb"] = new Error("Could not initialize database", exception);
                Log.Error(exception);
            }
        }

        private void InitWebApi()
        {
            try
            {
                WebApp.Start<OWINWebAPIConfig>(_settings.Get<string>(DbSetting.Keys.BaseWebAPIAddress));
            }
            catch (Exception exception)
            {
                _errors["InitWebApi"] = new Error("Unable to initialize WebAPI, check the hostname and the port number.", exception);
                Log.Error(exception);
            }
        }

        private void InitSignalR()
        {
            try
            {
                WebApp.Start<OWINSignalRConfig>(_settings.Get<string>(DbSetting.Keys.BaseWebSignalRAddress));

                try
                {
                    SignalRConnection.Init();
                }
                catch (Exception exception)
                {
                    _errors["InitSignalR"] = new Error("Unable to initialize SignalR, check the hostname and the port number.", exception);
                    Log.Error(exception);
                }
            }
            catch (Exception exception)
            {
                _errors["InitSignalR"] = new Error("Unable to start SignalR server, check the hostname and the port number.", exception);
                Log.Error(exception);
            }
        }
    }
}
