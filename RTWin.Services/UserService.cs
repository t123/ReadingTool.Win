using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using NPoco.FluentMappings;
using RTWin.Entities;

namespace RTWin.Services
{
    public class UserService
    {
        private readonly Database _db;

        public UserService(Database db)
        {
            _db = db;
        }

        public User FindOne(long id)
        {
            return _db.SingleOrDefaultById<User>(id);
        }

        public User FindOneByUsername(string username)
        {
            string test = (username ?? "").Trim();
            return _db.FetchBy<User>(sql => sql.Where(x => x.Username == test).Limit(1)).FirstOrDefault();
        }

        public void Save(User user)
        {
            if (user.UserId == 0)
            {
                _db.Insert(user);
            }
            else
            {
                _db.Update(user);
            }
        }

        public void DeleteOne(long id)
        {
            var languages = _db.FetchWhere<Language>(x => x.UserId == id);

            foreach (var language in languages)
            {
                _db.DeleteWhere<Item>(x => x.L1LanguageId == language.LanguageId);
                _db.DeleteWhere<Term>(x => x.LanguageId == language.LanguageId);
                _db.DeleteWhere<TermLog>(x => x.LanguageId == language.LanguageId);
                _db.DeleteWhere<LanguagePlugin>(x => x.LanguageId == language.LanguageId);
                _db.Delete<Language>(language.LanguageId);
            }

            _db.DeleteWhere<User>(x => x.UserId == id);
        }

        public IEnumerable<User> FindAll()
        {
            return _db.FetchBy<User>(sql => sql.OrderBy(x => x.Username));
        }
    }
}
