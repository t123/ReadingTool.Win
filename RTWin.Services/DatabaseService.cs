using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using RTWin.Entities;

namespace RTWin.Services
{
    public class DatabaseService
    {
        private readonly Database _db;
        private readonly DbSettingService _settings;

        public DatabaseService(Database db, DbSettingService settings)
        {
            _db = db;
            _settings = settings;
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
CREATE TABLE ""user"" (""UserId"" INTEGER PRIMARY KEY ,""Username"" TEXT NOT NULL ,""LastLogin"" TEXT, ""AccessKey"" TEXT, ""AccessSecret"" TEXT, ""SyncData"" BOOL)
");

            sql.Add("language", @"
CREATE TABLE ""language"" (""LanguageId"" INTEGER PRIMARY KEY ,""Name"" TEXT,""DateCreated"" TEXT,""DateModified"" TEXT,""IsArchived"" INTEGER,""LanguageCode"" TEXT,""UserId"" TEXT, ""Direction"" INTEGER, ""SentenceRegex"" TEXT, ""TermRegex"" TEXT)
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
                _settings.Save(new DbSetting
                {
                    Key = DbSetting.Keys.BaseWebAPIAddress,
                    Value = "http://localhost:9000"
                });
            }

            if (key2.Count == 0)
            {
                _settings.Save(new DbSetting
                {
                    Key = DbSetting.Keys.BaseWebSignalRAddress,
                    Value = "http://localhost:8888"
                });
            }

            if (key3.Count == 0)
            {
                _settings.Save(new DbSetting
                {
                    Key = DbSetting.Keys.ApiServer,
                    Value = "http://rt3/"
                });
            }

            var userCount = _db.ExecuteScalar<int>("SELECT COUNT(*) FROM User");
            if (userCount == 0)
            {
                _db.Insert(User.NewUser());
            }
        }
    }
}
