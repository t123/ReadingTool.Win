using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NPoco;
using NPoco.FluentMappings;
using RTWin.Entities;
using RTWin.Entities.Enums;

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
                item.DateCreated = DateTime.UtcNow;
                item.DateModified = DateTime.UtcNow;
                _db.Insert(item);
            }
            else
            {
                item.DateModified = DateTime.UtcNow;
                _db.Update(item);
            }
        }

        public void DeleteOne(long id)
        {
            _db.Delete<Item>(id);
        }

        public IEnumerable<Item> FindAll()
        {
            var items = _db.Fetch<Item>("WHERE UserId=@0 ORDER BY CollectionName, CollectionNo, L1Title", _user.UserId);

            return items;
        }

        public IEnumerable<string> FindCollectionsPerLanguage()
        {
            List<string> list = _db.FetchBy<Language>(sql => sql.Where(x => !x.IsArchived && x.UserId == _user.UserId).OrderBy("ORDER BY Name COLLATE NOCASE"))
                .Select(x => "\"" + x.Name + "\"")
                .ToList();

            var builder = Sql.Builder.Append(@"SELECT DISTINCT('""' || b.Name || '"" - ""' || a.CollectionName || '""') as cName FROM item a, language b");
            builder.Append("WHERE a.L1Languageid=b.LanguageId AND a.Userid=@0 and b.IsArchived=0", _user.UserId);
            builder.OrderBy("cName COLLATE NOCASE");

            list.AddRange(_db.Fetch<string>(builder));

            return list;
        }

        public IEnumerable<string> FindAllCollectionNames(long? languageId)
        {
            var builder = Sql.Builder.Append("SELECT DISTINCT(CollectionName) as X FROM item");
            builder.Append("WHERE UserId=@0", _user.UserId);

            if (languageId.HasValue)
            {
                builder.Append(" AND L1LanguageId=@0", languageId.Value);
            }

            builder.OrderBy("x COLLATE NOCASE");

            return _db.Fetch<string>(builder);
        }

        public void MarkLastRead(long id)
        {
            var item = FindOne(id);

            if (item == null)
            {
                return;
            }

            item.LastRead = DateTime.UtcNow;
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

        public Tuple<Item, Item> FindNextPrev(Item item)
        {
            if (item == null)
            {
                return new Tuple<Item, Item>(null, null);
            }

            var next = _db.FetchWhere<Item>(x => x.L1LanguageId == item.L1LanguageId && x.CollectionName == item.CollectionName && x.CollectionNo > item.CollectionNo).OrderBy(x => x.CollectionNo).FirstOrDefault();
            var prev = _db.FetchWhere<Item>(x => x.L1LanguageId == item.L1LanguageId && x.CollectionName == item.CollectionName && x.CollectionNo < item.CollectionNo).OrderBy(x => x.CollectionNo).LastOrDefault();

            return new Tuple<Item, Item>(next, prev);
        }

        public IEnumerable<Item> Search(
            ItemType? itemType = null,
            DateTime? modified = null,
            string collectionName = null,
            string title = null,
            long? languageId = null,
            bool? isParallel = null,
            bool? hasMedia = null,
            string filter = null,
            int? maxResults = null
        )
        {
            var sql = Sql.Builder.Append("SELECT item.*, B.Name as L1Language, C.Name as L2Language FROM item item");
            sql.LeftJoin("language B on item.L1LanguageId=B.LanguageId");
            sql.LeftJoin("language C on item.L2LanguageId=C.LanguageId");
            sql.Append("WHERE item.UserId=@0 ", _user.UserId);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var matches = Regex.Matches(filter, @"[\#\w]+|""[\w\s]*""");
                string add = "";

                foreach (Match match in matches)
                {
                    if (match.Value.StartsWith("#"))
                    {
                        if (match.Value.Length > 1)
                        {
                            var remainder = match.Value.Substring(1, match.Length - 1).ToLowerInvariant();

                            switch (remainder)
                            {
                                case "parallel":
                                    isParallel = true;
                                    break;

                                case "media":
                                    hasMedia = true;
                                    break;

                                case "text":
                                    itemType = ItemType.Text;
                                    break;

                                case "video":
                                    itemType = ItemType.Video;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        string value = match.Value.Trim();

                        if (value.StartsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 1);
                        }

                        if (value.EndsWith("\""))
                        {
                            value = value.Substring(0, value.Length - 1);
                        }

                        if (value.Length > 0)
                        {
                            add += string.Format("(item.CollectionName LIKE '{0}%' OR item.L1Title LIKE '{0}%' OR L1Language LIKE '{0}%') AND ", value.Replace("'", "''"));
                        }
                    }
                }

                if (add.Length > 0)
                {
                    sql.Append(" AND " + add.Substring(0, add.Length - 4));
                }
            }

            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                sql.Append("AND item.CollectionName LIKE @0", collectionName + "%");
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                sql.Append("AND item.L1Title LIKE @0", title + "%");
            }

            if (isParallel.HasValue)
            {
                if (isParallel.Value)
                {
                    sql.Append("AND (item.L2Content IS NOT NULL AND item.L2Content<>'')");
                }
                else
                {
                    sql.Append("AND (item.L2Content IS NULL OR item.L2Content='')");
                }
            }

            if (hasMedia.HasValue)
            {
                if (hasMedia.Value)
                {
                    sql.Append("AND (item.MediaUri IS NOT NULL AND item.MediaUri<>'')");
                }
                else
                {
                    sql.Append("AND (item.MediaUri IS NULL OR item.MediaUri='')");
                }
            }

            if (itemType.HasValue)
            {
                sql.Append("AND item.ItemType=@0", itemType.Value);
            }

            if (languageId.HasValue)
            {
                sql.Append("AND item.L1LanguageId=@0", languageId.Value);
            }

            if (modified.HasValue)
            {
                sql.Append("AND item.DateModified>=@0", modified.Value.ToUniversalTime());
            }

            sql.OrderBy("L1Language, item.CollectionName, item.CollectionNo, item.L1Title, item.ItemId");

            if (maxResults.HasValue && maxResults > 0)
            {
                sql.Append("LIMIT @0", maxResults.Value);
            }

            var results = _db.Query<Item>(sql);
            return results;
        }

        public IEnumerable<Item> FindRecent(int? maxResults)
        {
            var sql = Sql.Builder.Append("SELECT item.*, B.Name as L1Language, C.Name as L2Language FROM item item");
            sql.LeftJoin("language B on item.L1LanguageId=B.LanguageId");
            sql.LeftJoin("language C on item.L2LanguageId=C.LanguageId");
            sql.Append("WHERE item.UserId=@0 ", _user.UserId);
            sql.OrderBy("item.LastRead DESC");

            if (maxResults.HasValue && maxResults > 0)
            {
                sql.Append("LIMIT @0", maxResults.Value);
            }

            var results = _db.Query<Item>(sql);
            return results;
        }
    }
}
