using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using RTWin.Entities;

namespace RTWin.Services
{
    public class DatabaseService
    {
        private readonly Database _db;

        public DatabaseService(Database db)
        {
            _db = db;
        }

        private Dictionary<string, string> GetAndCacheSettings()
        {
            ObjectCache cache = MemoryCache.Default;

            Dictionary<string, string> settings = (Dictionary<string, string>)cache.Get("DbSetttings");

            if (settings == null)
            {
                settings = _db.Fetch<DbSetting>().ToDictionary(x => x.Key.ToLowerInvariant(), x => x.Value);
                cache.Add("DbSettings", settings, ObjectCache.InfiniteAbsoluteExpiration);
            }

            return settings;
        }

        public void SaveSetting(DbSetting dbSetting)
        {
            if (dbSetting == null)
            {
                return;
            }

            SaveSetting(dbSetting.Key, dbSetting.Value);
        }

        public void SaveSetting(string key, object value)
        {
            key = NormalizeKey(key);
            var setting = _db.FetchWhere<DbSetting>(x => x.Key == key).FirstOrDefault();

            if (setting == null)
            {
                setting = new DbSetting()
                {
                    Key = key,
                    Value = (value ?? "").ToString()
                };

                _db.Insert(setting);
            }
            else
            {
                setting.Value = (value ?? "").ToString();
                _db.Update(setting);
            }

            ObjectCache cache = MemoryCache.Default;
            cache.Remove("DbSettings");
        }

        public Dictionary<string, string> GetSettings()
        {
            return GetAndCacheSettings();
        }

        public string GetSetting(string key)
        {
            key = NormalizeKey(key);
            var setting = GetAndCacheSettings().FirstOrDefault(x => x.Key == key);
            return setting.Value;
        }

        public T GetSetting<T>(string key)
        {
            key = NormalizeKey(key);
            var settings = GetAndCacheSettings();
            var setting = settings.ContainsKey(key) ? settings[key] : null;

            Type t = typeof(T);
            t = Nullable.GetUnderlyingType(t) ?? t;

            return (setting == null) ? default(T) : (T)Convert.ChangeType(setting, t);
        }

        private string NormalizeKey(string key)
        {
            return (key ?? "").ToLowerInvariant().Trim();
        }

        public void CreateAndUpgradeDatabase()
        {
            if (!TableExists("dbversion"))
            {
                CreateDatabase();
            }

            UpgradeDatabase();
            CheckSettings();
        }

        private void UpgradeDatabase()
        {
            var version = _db.FetchBy<DbVersion>(sql => sql.Limit(1)).FirstOrDefault();

            if (version == null)
            {
                CreateDatabase();
                version = _db.FetchBy<DbVersion>(sql => sql.Limit(1)).FirstOrDefault();

                if (version == null)
                {
                    throw new Exception("Something is terribly wrong with the database");
                }
            }

            switch (version.Version)
            {
                case 1:
                    break;

                default:
                    break;
            }
        }

        //private void DropAllTables()
        //{
        //    string[] names = new[] { "language", "term", "languagecode", "item", "user", "dbversion" };

        //    foreach (var name in names)
        //    {
        //        if (TableExists(name))
        //        {
        //            _db.Execute(string.Format(@"DROP TABLE ""main"".""{0}""", name));
        //        }
        //    }
        //}

        private bool TableExists(string tableName)
        {
            int rows = _db.ExecuteScalar<int>("SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name=@0", tableName);
            return rows > 0;
        }

        private bool IndexExists(string indexName)
        {
            int rows = _db.ExecuteScalar<int>("SELECT COUNT(name) FROM sqlite_master WHERE type='index' AND name=@0", indexName);
            return rows > 0;
        }

        private void CreateIndexes()
        {
            Dictionary<string, string> sql = new Dictionary<string, string>();

            sql.Add("term_index1", @"CREATE UNIQUE INDEX ""LowerPhrase_Language"" ON ""term"" (""LowerPhrase"" ASC, ""LanguageId"" ASC)");
            sql.Add("pluginstorage_index", @"CREATE UNIQUE INDEX ""main"".""Plugin_Storage_UUID"" ON ""plugin_storage"" (""Uuid"" ASC, ""Key"" ASC)");

            foreach (var kvp in sql)
            {
                if (!IndexExists(kvp.Key))
                {
                    _db.Execute(kvp.Value);
                }
            }
        }

        private void CreateDatabase()
        {
            Dictionary<string, string> sql = new Dictionary<string, string>();

            sql.Add("dbversion", @"
CREATE TABLE ""dbversion"" (""DbVersionId"" INTEGER PRIMARY KEY  NOT NULL , ""Version"" INTEGER NOT NULL )
");

            sql.Add("settings", @"
CREATE  TABLE ""settings"" (""Key"" TEXT UNIQUE , ""Value"" TEXT)
");

            sql.Add("user", @"
CREATE TABLE ""user"" (""UserId""  INTEGER PRIMARY KEY, ""Username"" TEXT NOT NULL  UNIQUE, ""LastLogin"" TEXT, ""JsonSettings"" TEXT )
");

            sql.Add("language", @"
CREATE TABLE language (
    ""LanguageId"" INTEGER PRIMARY KEY,
    ""Name"" TEXT,
    ""DateCreated"" TEXT,
    ""DateModified"" TEXT,
    ""IsArchived"" INTEGER,
    ""LanguageCode"" TEXT,
    ""JsonSettings"" TEXT,
    ""UserId"" TEXT
);
");

            sql.Add("term", @"
CREATE TABLE ""term"" (
    ""TermId"" INTEGER PRIMARY KEY,
    ""DateCreated"" TEXT,
    ""DateModified"" TEXT,
    ""Phrase"" TEXT,
    ""BasePhrase"" TEXT,
    ""LowerPhrase"" TEXT,
    ""Definition"" TEXT,
    ""Sentence"" TEXT,
    ""State"" INTEGER,
    ""LanguageId"" INTEGER,
    ""ItemSourceId"" INTEGER,
    ""UserId"" TEXT
);
");

            sql.Add("languagecode", @"CREATE TABLE ""languagecode"" (""Name"" TEXT, ""Code"" TEXT UNIQUE)");

            sql.Add("item", @"CREATE TABLE ""item"" (""ItemId"" INTEGER PRIMARY KEY  NOT NULL , ""ItemType"" INTEGER, ""CollectionName"" TEXT, ""CollectionNo"" INTEGER, ""L1Title"" TEXT, ""L1Content"" TEXT, ""L1LanguageId"" INTEGER, ""L2Title"" TEXT, ""L2Content"" TEXT, ""L2LanguageId"" INTEGER, ""DateCreated"" TEXT, ""DateModified"" TEXT, ""LastRead"" TEXT, ""MediaUri"" TEXT, ""UserId"" TEXT, ""ReadTimes"" INTEGER, ""ListenedTimes"" INTEGER)");

            sql.Add("termlog", @"CREATE TABLE ""termlog"" (""EntryDate"" TEXT, ""TermId"" INTEGER, ""State"" INTEGER, ""Type"" INTEGER DEFAULT 0, ""LanguageId"" INTEGER, ""UserId"" TEXT)");

            sql.Add("plugin", @"CREATE TABLE ""plugin"" (""PluginId"" INTEGER PRIMARY KEY  NOT NULL , ""Name"" TEXT, ""Description"" TEXT, ""Content"" TEXT, ""UUID"" TEXT)");

            sql.Add("pluginstorage", @"CREATE TABLE ""plugin_storage"" (""Id"" INTEGER PRIMARY KEY AUTOINCREMENT  NOT NULL , ""Uuid"" TEXT, ""Key"" TEXT, ""Value"" TEXT)");

            foreach (var kvp in sql)
            {
                if (!TableExists(kvp.Key))
                {
                    _db.Execute(kvp.Value);
                }
            }

            List<LanguageCode> codes = new List<LanguageCode>()
            {
                new LanguageCode {Name = "Afrikaans", Code = "af"},
                new LanguageCode {Name = "Albanian", Code = "sq"},
                //new LanguageCode {Name = "Arabic", Code = "ar"},
                new LanguageCode {Name = "Azerbaijani", Code = "az"},
                new LanguageCode {Name = "Basque", Code = "eu"},
                new LanguageCode {Name = "Bengali", Code = "bn"},
                new LanguageCode {Name = "Belarusian", Code = "be"},
                new LanguageCode {Name = "Bulgarian", Code = "bg"},
                new LanguageCode {Name = "Catalan", Code = "ca"},
                //new LanguageCode {Name = "Chinese Simplified", Code = "zh-CN"},
                //new LanguageCode {Name = "Chinese Traditional", Code = "zh-TW"},
                new LanguageCode {Name = "Croatian", Code = "hr"},
                new LanguageCode {Name = "Czech", Code = "cs"},
                new LanguageCode {Name = "Danish", Code = "da"},
                new LanguageCode {Name = "Dutch", Code = "nl"},
                new LanguageCode {Name = "English", Code = "en"},
                new LanguageCode {Name = "Esperanto", Code = "eo"},
                new LanguageCode {Name = "Estonian", Code = "et"},
                new LanguageCode {Name = "Filipino", Code = "tl"},
                new LanguageCode {Name = "Finnish", Code = "fi"},
                new LanguageCode {Name = "French", Code = "fr"},
                new LanguageCode {Name = "Galician", Code = "gl"},
                new LanguageCode {Name = "Georgian", Code = "ka"},
                new LanguageCode {Name = "German", Code = "de"},
                new LanguageCode {Name = "Greek", Code = "el"},
                new LanguageCode {Name = "Gujarati", Code = "gu"},
                new LanguageCode {Name = "Haitian Creole", Code = "ht"},
                new LanguageCode {Name = "Hebrew", Code = "iw"},
                new LanguageCode {Name = "Hindi", Code = "hi"},
                new LanguageCode {Name = "Hungarian", Code = "hu"},
                new LanguageCode {Name = "Icelandic", Code = "is"},
                new LanguageCode {Name = "Indonesian", Code = "id"},
                new LanguageCode {Name = "Irish", Code = "ga"},
                new LanguageCode {Name = "Italian", Code = "it"},
                //new LanguageCode {Name = "Japanese", Code = "ja"},
                new LanguageCode {Name = "Kannada", Code = "kn"},
                new LanguageCode {Name = "Korean", Code = "ko"},
                new LanguageCode {Name = "Latin", Code = "la"},
                new LanguageCode {Name = "Latvian", Code = "lv"},
                new LanguageCode {Name = "Lithuanian", Code = "lt"},
                new LanguageCode {Name = "Macedonian", Code = "mk"},
                new LanguageCode {Name = "Malay", Code = "ms"},
                new LanguageCode {Name = "Maltese", Code = "mt"},
                new LanguageCode {Name = "Norwegian", Code = "no"},
                new LanguageCode {Name = "Persian", Code = "fa"},
                new LanguageCode {Name = "Polish", Code = "pl"},
                new LanguageCode {Name = "Portuguese", Code = "pt"},
                new LanguageCode {Name = "Romanian", Code = "ro"},
                new LanguageCode {Name = "Russian", Code = "ru"},
                new LanguageCode {Name = "Serbian", Code = "sr"},
                new LanguageCode {Name = "Slovak", Code = "sk"},
                new LanguageCode {Name = "Slovenian", Code = "sl"},
                new LanguageCode {Name = "Spanish", Code = "es"},
                new LanguageCode {Name = "Swahili", Code = "sw"},
                new LanguageCode {Name = "Swedish", Code = "sv"},
                new LanguageCode {Name = "Tamil", Code = "ta"},
                new LanguageCode {Name = "Telugu", Code = "te"},
                //new LanguageCode {Name = "Thai", Code = "th"},
                new LanguageCode {Name = "Turkish", Code = "tr"},
                new LanguageCode {Name = "Ukrainian", Code = "uk"},
                new LanguageCode {Name = "Urdu", Code = "ur"},
                new LanguageCode {Name = "VietCodese", Code = "vi"},
                new LanguageCode {Name = "Welsh", Code = "cy"},
                new LanguageCode {Name = "Yiddish", Code = "yi"},
                new LanguageCode {Code = "--", Name = "Not Set"},
            };

            _db.Execute("DELETE FROM LanguageCode");
            _db.Execute("DELETE FROM DbVersion");

            foreach (var lc in codes)
            {
                _db.Insert(lc);
            }

            _db.Insert(new DbVersion() { Version = 1 });

            CreateIndexes();
        }

        private void CheckSettings()
        {
            var key1 = _db.Fetch<object>("SELECT * FROM Settings WHERE Key=@0", DbSetting.Keys.BaseWebAPIAddress);
            var key2 = _db.Fetch<object>("SELECT * FROM Settings WHERE Key=@0", DbSetting.Keys.BaseWebSignalRAddress);
            var key3 = _db.Fetch<object>("SELECT * FROM Settings WHERE Key=@0", DbSetting.Keys.ApiServer);

            if (key1.Count == 0)
            {
                SaveSetting(new DbSetting
                {
                    Key = DbSetting.Keys.BaseWebAPIAddress,
                    Value = "http://localhost:9000"
                });
            }

            if (key2.Count == 0)
            {
                SaveSetting(new DbSetting
                {
                    Key = DbSetting.Keys.BaseWebSignalRAddress,
                    Value = "http://localhost:8888"
                });
            }

            if (key3.Count == 0)
            {
                SaveSetting(new DbSetting
                {
                    Key = DbSetting.Keys.ApiServer,
                    Value = "http://rt3/"
                });
            }

            var userCount = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM User");
            if (userCount == 0)
            {
                _db.Insert(new User()
                {
                    Username = "Default User",
                    LastLogin = DateTime.UtcNow,
                    Settings = new UserSettings()
                    {
                        AccessKey = "",
                        AccessSecret = "",
                        SyncData = false
                    }
                });
            }
        }
    }
}
