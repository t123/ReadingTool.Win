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

        public void Save(Language language)
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
        }

        public Language FindOne(long id)
        {
            return _db.FirstOrDefault<Language>("SELECT * FROM Language WHERE LanguageId=@0 AND UserId=@1", id, _user.UserId);
        }

        public void DeleteOne(long id)
        {
            _db.Delete<Language>(id);
        }

        public IList<Language> FindAll()
        {
            return _db.FetchBy<Language>(sql => sql.Where(x => x.UserId == _user.UserId).OrderBy(x => !x.IsArchived).ThenBy(x => x.Name));
        }
    }
}
