using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using RTWin.Entities;

namespace RTWin.Services
{
    public class PluginService
    {
        private readonly Database _db;
        private readonly User _user;

        public PluginService(Database db, User user)
        {
            _db = db;
            _user = user;
        }

        public Plugin FindOne(long id)
        {
            return _db.SingleOrDefaultById<Plugin>(id);
        }

        public void Save(Plugin plugin)
        {
            if (plugin.PluginId == 0)
            {
                _db.Insert(plugin);
            }
            else
            {
                _db.Update(plugin);
            }
        }

        public void DeleteOne(long id)
        {
            _db.Delete<Plugin>(id);
        }

        public IList<Plugin> FindAll()
        {
            return _db.FetchBy<Plugin>(sql => sql.OrderBy(x => x.Name));
        }

        public IList<Plugin> FindAllForLanguage(long languageId)
        {
            var result = _db.Query<Plugin>("SELECT a.* FROM plugin a, language_plugin b WHERE a.PluginId=b.PluginId AND b.LanguageId=@0", languageId);
            return result.ToList();
        }
    }
}
