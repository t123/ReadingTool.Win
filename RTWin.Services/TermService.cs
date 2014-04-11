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
    public class TermService
    {
        private readonly Database _db;
        private readonly User _user;

        public TermService(Database db, User user)
        {
            _db = db;
            _user = user;
        }

        public Term FindOne(long id)
        {
            return _db.SingleOrDefaultById<Term>(id);
        }

        public Term FindOneByPhraseAndLanguage(string phrase, long languageId)
        {
            string lower = (phrase ?? "").ToLowerInvariant().Trim();
            return _db.FirstOrDefault<Term>("SELECT * FROM term WHERE LowerPhrase=@0 and LanguageId=@1 AND UserId=@2", lower, languageId, _user.UserId);
        }

        public IList<Term> FindAllByLanguage(long languageId)
        {
            return _db.Fetch<Term>("WHERE LanguageId=@0 AND UserId=@1", languageId, _user.UserId);
        }

        public void Save(Term term)
        {
            if (term.TermId == 0)
            {
                term.UserId = _user.UserId;
                term.DateCreated = DateTime.Now;
                term.DateModified = DateTime.Now;
                _db.Insert(term);
            }
            else
            {
                term.DateModified = DateTime.Now;
                _db.Update(term);
            }
        }

        public void DeleteOne(long id)
        {
            _db.Delete<Term>(id);
        }
    }
}
