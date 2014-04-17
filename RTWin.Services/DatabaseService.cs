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

        private void DropAllTables()
        {
            string[] names = new[] { "language", "term", "languagecode", "item", "user", "dbversion" };

            foreach (var name in names)
            {
                if (TableExists(name))
                {
                    _db.Execute(string.Format(@"DROP TABLE ""main"".""{0}""", name));
                }
            }
        }

        private bool TableExists(string tableName)
        {
            int rows = _db.ExecuteScalar<int>("SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name=@0", tableName);
            return rows > 0;
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
CREATE TABLE ""user"" (""UserId"" INTEGER PRIMARY KEY AUTOINCREMENT , ""Username"" TEXT NOT NULL  UNIQUE )
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

            sql.Add("term_index1", @"
CREATE UNIQUE INDEX ""LowerPhrase_Language"" ON ""term"" (""LowerPhrase"" ASC, ""LanguageId"" ASC)
");

            sql.Add("languagecode", @"
CREATE TABLE ""languagecode"" (""LanguageCodeId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""Name"" TEXT, ""Code"" TEXT)
");

            sql.Add("item", @"CREATE TABLE ""item"" (""ItemId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""ItemType"" INTEGER, ""CollectionName"" TEXT, ""CollectionNo"" INTEGER, ""L1Title"" TEXT, ""L1Content"" TEXT, ""L1LanguageId"" INTEGER, ""L2Title"" TEXT, ""L2Content"" TEXT, ""L2LanguageId"" INTEGER, ""DateCreated"" TEXT, ""DateModified"" TEXT, ""LastRead"" TEXT, ""MediaUri"" TEXT, ""UserId"" INTEGER, ""ReadTimes"" INTEGER, ""ListenedTimes"" INTEGER)");

            sql.Add("termlog", @"CREATE TABLE ""termlog"" (""Id"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""EntryDate"" TEXT, ""TermId"" INTEGER, ""State"" INTEGER, ""Type"" INTEGER DEFAULT 0, ""LanguageId"" INTEGER)");

            sql.Add("plugin", @"CREATE TABLE ""plugin"" (""PluginId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""Name"" TEXT, ""Description"" TEXT, ""Content"" TEXT, ""UUID"" TEXT)");

            foreach (var kvp in sql)
            {
                if (!TableExists(kvp.Key))
                {
                    _db.Execute(kvp.Value);
                }
            }

            List<LanguageCode> codes = new List<LanguageCode>()
            {
                new LanguageCode() {Code = "en", Name = "English"},
                new LanguageCode() {Code = "de", Name = "German"},
                new LanguageCode() {Code = "fr", Name = "French"},
                new LanguageCode() {Code = "af", Name = "Afrikaans"},
            };

            _db.Execute("DELETE FROM LanguageCode");
            _db.Execute("DELETE FROM DbVersion");

            foreach (var lc in codes)
            {
                _db.Insert(lc);
            }

            _db.Insert(new DbVersion() { Version = 1 });
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
