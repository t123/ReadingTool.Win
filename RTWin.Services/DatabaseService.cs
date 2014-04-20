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

            dbSetting.Key = dbSetting.Key.ToLowerInvariant().Trim();

            if (dbSetting.Id == 0)
            {
                _db.Insert(dbSetting);
            }
            else
            {
                _db.Update(dbSetting);
            }

            ObjectCache cache = MemoryCache.Default;
            cache.Remove("DbSettings");
        }

        public Dictionary<string, string> GetSettings()
        {
            return GetAndCacheSettings();
        }

        public string GetSetting(int id)
        {
            var setting = _db.SingleById<DbSetting>(id);
            return setting == null ? null : setting.Value;
        }

        public string GetSetting(string key)
        {
            key = key.ToLowerInvariant();
            var setting = GetAndCacheSettings().FirstOrDefault(x => x.Key == key);
            return setting.Value;
        }

        public T GetSetting<T>(string key)
        {
            key = key.ToLowerInvariant();
            var settings = GetAndCacheSettings();
            var setting = settings.ContainsKey(key) ? settings[key] : null;

            if (setting == null)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(setting, typeof(T));
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
CREATE  TABLE ""settings"" (""Id"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""SKey"" TEXT UNIQUE , ""SValue"" TEXT)
");

            sql.Add("user", @"
CREATE TABLE ""user"" (""UserId"" INTEGER PRIMARY KEY AUTOINCREMENT , ""Username"" TEXT NOT NULL  UNIQUE, ""JsonSettings"" TEXT )
");

            sql.Add("language", @"
CREATE TABLE language (
    ""LanguageId"" INTEGER PRIMARY KEY AUTOINCREMENT,
    ""Name"" TEXT,
    ""DateCreated"" TEXT,
    ""DateModified"" TEXT,
    ""IsArchived"" INTEGER,
    ""LanguageCode"" TEXT,
    ""JsonSettings"" TEXT,
    ""UserId"" INTEGER
);
");

            sql.Add("term", @"
CREATE TABLE ""term"" (
    ""TermId"" INTEGER PRIMARY KEY AUTOINCREMENT,
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
    ""UserId"" INTEGER
);
");

            sql.Add("languagecode", @"
CREATE TABLE ""languagecode"" (""LanguageCodeId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""Name"" TEXT, ""Code"" TEXT)
");

            sql.Add("item", @"CREATE TABLE ""item"" (""ItemId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""ItemType"" INTEGER, ""CollectionName"" TEXT, ""CollectionNo"" INTEGER, ""L1Title"" TEXT, ""L1Content"" TEXT, ""L1LanguageId"" INTEGER, ""L2Title"" TEXT, ""L2Content"" TEXT, ""L2LanguageId"" INTEGER, ""DateCreated"" TEXT, ""DateModified"" TEXT, ""LastRead"" TEXT, ""MediaUri"" TEXT, ""UserId"" INTEGER, ""ReadTimes"" INTEGER, ""ListenedTimes"" INTEGER)");

            sql.Add("termlog", @"CREATE TABLE ""termlog"" (""Id"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""EntryDate"" TEXT, ""TermId"" INTEGER, ""State"" INTEGER, ""Type"" INTEGER DEFAULT 0, ""LanguageId"" INTEGER, ""UserId"" INTEGER)");

            sql.Add("plugin", @"CREATE TABLE ""plugin"" (""PluginId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""Name"" TEXT, ""Description"" TEXT, ""Content"" TEXT, ""UUID"" TEXT)");

            sql.Add("pluginstorage", @"CREATE TABLE ""plugin_storage"" (""Id"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""Uuid"" TEXT, ""Key"" TEXT, ""Value"" TEXT)");

            foreach (var kvp in sql)
            {
                if (!TableExists(kvp.Key))
                {
                    _db.Execute(kvp.Value);
                }
            }

            List<LanguageCode> codes = new List<LanguageCode>()
            {
                new LanguageCode {Code = "Afrikaans", Name = "af"},
                new LanguageCode {Code = "Albanian", Name = "sq"},
                //new LanguageCode {Code = "Arabic", Name = "ar"},
                new LanguageCode {Code = "Azerbaijani", Name = "az"},
                new LanguageCode {Code = "Basque", Name = "eu"},
                new LanguageCode {Code = "Bengali", Name = "bn"},
                new LanguageCode {Code = "Belarusian", Name = "be"},
                new LanguageCode {Code = "Bulgarian", Name = "bg"},
                new LanguageCode {Code = "Catalan", Name = "ca"},
                //new LanguageCode {Code = "Chinese Simplified", Name = "zh-CN"},
                //new LanguageCode {Code = "Chinese Traditional", Name = "zh-TW"},
                new LanguageCode {Code = "Croatian", Name = "hr"},
                new LanguageCode {Code = "Czech", Name = "cs"},
                new LanguageCode {Code = "Danish", Name = "da"},
                new LanguageCode {Code = "Dutch", Name = "nl"},
                new LanguageCode {Code = "English", Name = "en"},
                new LanguageCode {Code = "Esperanto", Name = "eo"},
                new LanguageCode {Code = "Estonian", Name = "et"},
                new LanguageCode {Code = "Filipino", Name = "tl"},
                new LanguageCode {Code = "Finnish", Name = "fi"},
                new LanguageCode {Code = "French", Name = "fr"},
                new LanguageCode {Code = "Galician", Name = "gl"},
                new LanguageCode {Code = "Georgian", Name = "ka"},
                new LanguageCode {Code = "German", Name = "de"},
                new LanguageCode {Code = "Greek", Name = "el"},
                new LanguageCode {Code = "Gujarati", Name = "gu"},
                new LanguageCode {Code = "Haitian Creole", Name = "ht"},
                new LanguageCode {Code = "Hebrew", Name = "iw"},
                new LanguageCode {Code = "Hindi", Name = "hi"},
                new LanguageCode {Code = "Hungarian", Name = "hu"},
                new LanguageCode {Code = "Icelandic", Name = "is"},
                new LanguageCode {Code = "Indonesian", Name = "id"},
                new LanguageCode {Code = "Irish", Name = "ga"},
                new LanguageCode {Code = "Italian", Name = "it"},
                //new LanguageCode {Code = "Japanese", Name = "ja"},
                new LanguageCode {Code = "Kannada", Name = "kn"},
                new LanguageCode {Code = "Korean", Name = "ko"},
                new LanguageCode {Code = "Latin", Name = "la"},
                new LanguageCode {Code = "Latvian", Name = "lv"},
                new LanguageCode {Code = "Lithuanian", Name = "lt"},
                new LanguageCode {Code = "Macedonian", Name = "mk"},
                new LanguageCode {Code = "Malay", Name = "ms"},
                new LanguageCode {Code = "Maltese", Name = "mt"},
                new LanguageCode {Code = "Norwegian", Name = "no"},
                new LanguageCode {Code = "Persian", Name = "fa"},
                new LanguageCode {Code = "Polish", Name = "pl"},
                new LanguageCode {Code = "Portuguese", Name = "pt"},
                new LanguageCode {Code = "Romanian", Name = "ro"},
                new LanguageCode {Code = "Russian", Name = "ru"},
                new LanguageCode {Code = "Serbian", Name = "sr"},
                new LanguageCode {Code = "Slovak", Name = "sk"},
                new LanguageCode {Code = "Slovenian", Name = "sl"},
                new LanguageCode {Code = "Spanish", Name = "es"},
                new LanguageCode {Code = "Swahili", Name = "sw"},
                new LanguageCode {Code = "Swedish", Name = "sv"},
                new LanguageCode {Code = "Tamil", Name = "ta"},
                new LanguageCode {Code = "Telugu", Name = "te"},
                //new LanguageCode {Code = "Thai", Name = "th"},
                new LanguageCode {Code = "Turkish", Name = "tr"},
                new LanguageCode {Code = "Ukrainian", Name = "uk"},
                new LanguageCode {Code = "Urdu", Name = "ur"},
                new LanguageCode {Code = "Vietnamese", Name = "vi"},
                new LanguageCode {Code = "Welsh", Name = "cy"},
                new LanguageCode {Code = "Yiddish", Name = "yi"},
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
            var key1 = _db.Fetch<object>("SELECT * FROM Settings WHERE SKey=@0", DbSetting.Keys.BaseWebAPIAddress);
            var key2 = _db.Fetch<object>("SELECT * FROM Settings WHERE SKey=@0", DbSetting.Keys.BaseWebSignalRAddress);

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
        }
    }
}
