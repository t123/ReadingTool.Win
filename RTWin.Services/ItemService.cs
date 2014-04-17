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

        public string ChangeStatistics(Item item, string type, int amount)
        {
            if (item == null)
            {
                return "Not changed";
            }

            string message = string.Empty;

            if (type == "read")
            {
                if (amount > 0)
                {
                    item.ReadTimes++;
                }
                else if (amount < 0 && item.ReadTimes > 0)
                {
                    item.ReadTimes--;
                }

                message = "Item read " + item.ReadTimes + " times";
            }
            else if (type == "listen")
            {
                if (amount > 0)
                {
                    item.ListenedTimes++;
                }
                else if (amount < 0 && item.ListenedTimes > 0)
                {
                    item.ListenedTimes--;
                }

                if (item.ItemType == ItemType.Text)
                {
                    message = "Item listened to " + item.ListenedTimes + " times";
                }
                else
                {
                    message = "Item watched " + item.ListenedTimes + " times";
                }
            }

            _db.Update(item);
            return message;
        }

        public IEnumerable<Item> Search(ItemType? itemType, DateTime? modified, string collectionName, string title, long? languageId, bool? isParallel, bool? hasMedia)
        {
            var sql = Sql.Builder.Append("SELECT * FROM item");
            sql.Append("WHERE UserId=@0", _user.UserId);

            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                sql.Append("WHERE CollectionName LIKE @0", collectionName + "%");
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                sql.Append("WHERE L1Title LIKE @0", title + "%");
            }

            if (isParallel.HasValue)
            {
                if (isParallel.Value)
                {
                    sql.Append("WHERE (L2Content IS NOT NULL AND L2Content<>'')");
                }
                else
                {
                    sql.Append("WHERE (L2Content IS NULL OR L2Content='')");
                }
            }

            if (hasMedia.HasValue)
            {
                if (hasMedia.Value)
                {
                    sql.Append("WHERE (MediaUri IS NOT NULL AND MediaUri<>'')");
                }
                else
                {
                    sql.Append("WHERE (MediaUri IS NULL OR MediaUri='')");
                }
            }

            if (itemType.HasValue)
            {
                sql.Append("WHERE ItemType=@0", itemType.Value);
            }

            if (languageId.HasValue)
            {
                sql.Append("WHERE L1LanguageId=@0", languageId.Value);
            }

            if (modified.HasValue)
            {
                sql.Append("WHERE DateModified>=@0", modified);
            }

            sql.OrderBy("ItemId");

            return _db.Query<Item>(sql).ToList();
        }
    }
}
