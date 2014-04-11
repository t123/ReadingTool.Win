using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTWin.Entities
{
    public sealed class AppSettings
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class InternalSettings
        {
            public InternalSite Site { get; set; }
            public InternalDatabase Database { get; set; }
            public InternalMisc Misc { get; set; }

            public class InternalSite
            {
                public string CacheDirectory { get; set; }

                public InternalSite()
                {
                }
            }

            public class InternalDatabase
            {
                public string Name { get; set; }
                public string ConnectionString { get; set; }
            }



            public class InternalMisc
            {
                public Dictionary<string, object> Values { get; private set; }
 
                public InternalMisc()
                {
                    Values = new Dictionary<string, object>();
                }

                public T Get<T>(string key)
                {
                    object value;
                    if(Values.TryGetValue(key, out value))
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }

                    throw new KeyNotFoundException();
                }

                public object Get(string key)
                {
                    object value;
                    if(Values.TryGetValue(key, out value))
                    {
                        return value;
                    }

                    throw new KeyNotFoundException();
                }

                public T TryGet<T>(string key, T defaultValue)
                {
                    object value;
                    if(Values.TryGetValue(key, out value))
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }

                    return defaultValue;
                }

                public object TryGet(string key, object defaultValue)
                {
                    object value;
                    if(Values.TryGetValue(key, out value))
                    {
                        return value;
                    }

                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// <see cref="http://csharpindepth.com/Articles/General/Singleton.aspx"/>
        /// </summary>
        private static readonly Lazy<AppSettings> _lazy = new Lazy<AppSettings>(() => new AppSettings());
        public static AppSettings Instance { get { return _lazy.Value; } }
        public static InternalSettings Settings { get { return Instance.ISettings; } }

        public InternalSettings ISettings { get; private set; }
        private FileSystemWatcher _watcher = new FileSystemWatcher();
         
        private AppSettings()
        {
            Logger.Info("AppSettings initialsing");
            string configFile = ConfigurationManager.AppSettings["RTWin.Config"] ?? "default.config";
            if(string.IsNullOrEmpty(configFile)) throw new ArgumentException("Config file not specified");

            FileInfo configFI = new FileInfo(configFile);

            if(configFI == null || !configFI.Exists)
            {
                throw new NoNullAllowedException(string.Format("Config file '{0}' not found", configFile));
            }

            LoadSettings(configFI.FullName);

            Logger.Debug("AppSettings initialsed");

            Logger.Debug("initialsing AppSettings filewatcher");
            _watcher = new FileSystemWatcher(configFI.DirectoryName);
            _watcher.Filter = configFI.Name;
            _watcher.IncludeSubdirectories = false;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            _watcher.Changed += WatcherOnChanged;
            _watcher.EnableRaisingEvents = true;
            Logger.Debug("AppSettings Filewatcher initialsed");
        }

        private void LoadSettings(string configFile)
        {
            Logger.Debug("Loading AppSettings");

            string json;
            using(var sr = new StreamReader(configFile, Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }

            Logger.Debug("Deserializing JSON");
            
            ISettings = JsonConvert.DeserializeObject<InternalSettings>(json);

            if(ISettings.Misc == null) ISettings.Misc = new InternalSettings.InternalMisc();

            Logger.Debug("Loaded AppSettings");
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Logger.Info("AppSettings have changed, reloading");
            LoadSettings(fileSystemEventArgs.FullPath);
        }
    }
}
