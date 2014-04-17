using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using RTWin.Entities;

namespace RTWin.Services
{
    public class LanguageService
    {
        private readonly Database _db;
        private readonly User _user;

        public LanguageService(Database db, User user)
        {
            _db = db;
            _user = user;
        }

        public void Save(Language language, IEnumerable<long> plugins = null)
        {
            if (language.LanguageId == 0)
            {
                language.UserId = _user.UserId;
                language.DateCreated = DateTime.Now;
                language.DateModified = DateTime.Now;
                _db.Insert(language);
            }
            else
            {
                language.DateModified = DateTime.Now;
                _db.Update(language);
            }

            if (plugins != null)
            {
                _db.Execute("DELETE FROM language_plugin WHERE LanguageId=@0", language.LanguageId);

                foreach (var plugin in plugins)
                {
                    _db.Execute("INSERT INTO language_plugin VALUES ( @0, @1 )", language.LanguageId, plugin);
                }
            }
        }

        public Language FindOne(long id)
        {
            return _db.FirstOrDefault<Language>("SELECT * FROM Language WHERE LanguageId=@0 AND UserId=@1", id, _user.UserId);
        }

        public void DeleteOne(long id)
        {
            _db.DeleteWhere<Item>(x => x.L1LanguageId == id);
            _db.DeleteWhere<Term>(x => x.LanguageId == id);
            _db.DeleteWhere<TermLog>(x => x.LanguageId == id);
            _db.DeleteWhere<LanguagePlugin>(x => x.LanguageId == id);
            _db.Delete<Language>(id);
        }

        public IList<Language> FindAll()
        {
            return _db.FetchBy<Language>(sql => sql.Where(x => x.UserId == _user.UserId).OrderBy("ORDER BY Name COLLATE NOCASE"));
        }

        public Tuple<int, int, int, int> FindStatistics(long languageId)
        {
            var result = new
            {
                TotalItems = _db.ExecuteScalar<int>("SELECT COUNT(ItemId) FROM item WHERE L1LanguageId=@0", languageId),
                TotalTerms = _db.ExecuteScalar<int>("SELECT COUNT(TermId) FROM term WHERE LanguageId=@0", languageId),
                TotalKnown = _db.ExecuteScalar<int>("SELECT COUNT(TermId) FROM term WHERE LanguageId=@0 AND State=@1", languageId, TermState.Known),
                TotalUnknown = _db.ExecuteScalar<int>("SELECT COUNT(TermId) FROM term WHERE LanguageId=@0 AND State=@1", languageId, TermState.Unknown)
            };

            return new Tuple<int, int, int, int>(result.TotalItems, result.TotalTerms, result.TotalKnown, result.TotalUnknown);
        }
    }
}
