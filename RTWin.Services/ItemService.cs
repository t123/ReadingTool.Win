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
    public class ItemService
    {
        private readonly Database _db;
        private readonly User _user;

        public ItemService(Database db, User user)
        {
            _db = db;
            _user = user;
        }

        public Item FindOne(long id)
        {
            return _db.SingleOrDefaultById<Item>(id);
        }


        public void Save(Item item)
        {
            if (item.ItemId == 0)
            {
                item.UserId = _user.UserId;
                item.DateCreated = DateTime.Now;
                item.DateModified = DateTime.Now;
                _db.Insert(item);
            }
            else
            {
                item.DateModified = DateTime.Now;
                _db.Update(item);
            }
        }

        public void DeleteOne(long id)
        {
            _db.Delete<Item>(id);
        }

        public IList<Item> FindAll()
        {
            var items = _db.Fetch<Item>("WHERE UserId=@0 ORDER BY CollectionName, CollectionNo, L1Title", _user.UserId);

            return items;
        }

        public IList<string> FindAllCollectionNames(long? languageId)
        {
            var builder = Sql.Builder.Append("SELECT DISTINCT(CollectionName) as X FROM item");
            builder.Append("WHERE UserId=@0", _user.UserId);

            if (languageId.HasValue)
            {
                builder.Append(" AND LanguageId=@0", languageId.Value);
            }

            builder.OrderBy("x");

            return _db.Fetch<string>(builder);
        }

        public void MarkLastRead(long id)
        {
            var item = FindOne(id);

            if (item == null)
            {
                return;
            }

            item.LastRead = DateTime.Now;
            _db.Update(item);
        }
    }
}
