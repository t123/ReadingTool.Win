using System;
using System.Collections.Generic;
using System.Linq;
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

        public void CreateAndUpgradeDatabase()
        {
            if (!TableExists("dbversion"))
            {
                CreateDatabase();
            }

            UpgradeDatabase();
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

            sql.Add("languagecode", @"
CREATE TABLE ""languagecode"" (""LanguageCodeId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""Name"" TEXT, ""Code"" TEXT)
");

            sql.Add("item", @"CREATE TABLE ""item"" (""ItemId"" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , ""ItemType"" INTEGER, ""CollectionName"" TEXT, ""CollectionNo"" INTEGER, ""L1Title"" TEXT, ""L1Content"" TEXT, ""L1LanguageId"" INTEGER, ""L2Title"" TEXT, ""L2Content"" TEXT, ""L2LanguageId"" INTEGER, ""DateCreated"" TEXT, ""DateModified"" TEXT, ""LastRead"" TEXT, ""MediaUri"" TEXT, ""UserId"" INTEGER)");

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
    }
}
