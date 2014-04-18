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
    public class LanguageCodeService
    {
        private readonly Database _db;

        public LanguageCodeService()
        {
            _db = new Database("db");
        }

        public IEnumerable<LanguageCode> FindAll()
        {
            return _db.FetchBy<LanguageCode>(x=>x.OrderBy(y=>y.Name));
        }
    }
}
