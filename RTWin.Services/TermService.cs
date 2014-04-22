using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NPoco;
using RTWin.Entities;
using RTWin.Entities.Enums;

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
            return _db.FirstOrDefault<Term>("SELECT * FROM term WHERE LowerPhrase=@0 and LanguageId=@1 AND UserId=@2 LIMIT 1", lower, languageId, _user.UserId);
        }

        public IList<Term> FindAllByLanguage(long languageId)
        {
            return _db.Fetch<Term>("WHERE LanguageId=@0 AND UserId=@1", languageId, _user.UserId);
        }

        public void Save(Term term)
        {
            var isNew = false;
            if (term.TermId == 0)
            {
                isNew = true;
                term.UserId = _user.UserId;
                term.DateCreated = DateTime.UtcNow;
                term.DateModified = DateTime.UtcNow;
                _db.Insert(term);
            }
            else
            {
                term.DateModified = DateTime.UtcNow;
                _db.Update(term);
            }

            _db.Insert(new TermLog()
            {
                EntryDate = DateTime.UtcNow,
                State = term.State,
                TermId = term.TermId,
                Type = isNew ? TermType.Create : TermType.Modify,
                LanguageId = term.LanguageId,
                UserId = term.UserId
            });
        }

        public void DeleteOne(long id)
        {
            var term = FindOne(id);

            if (term == null)
            {
                return;
            }

            _db.Insert(new TermLog()
            {
                EntryDate = DateTime.UtcNow,
                State = term.State,
                TermId = id,
                Type = TermType.Delete,
                LanguageId = term.LanguageId,
                UserId = term.UserId
            });

            _db.Delete<Term>(id);
        }

        public IList<Term> FindAll()
        {
            return _db.Fetch<Term>("WHERE UserId=@0 ORDER BY LowerPhrase", _user.UserId);
        }

        public IEnumerable<Term> Search(long? languageId = null, DateTime? modified = null, int? maxResults = null, string filter = null)
        {
            var sql = Sql.Builder.Append("SELECT term.*, b.name as Language, c.collectionNo || ' - ' || c.CollectionName || ' ' || c.L1Title as ItemSource FROM term term");
            sql.LeftJoin("language b on term.LanguageId=b.LanguageId");
            sql.LeftJoin("item c on term.ItemSourceId=c.ItemId");
            sql.Append("WHERE term.UserId=@0", _user.UserId);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var matches = Regex.Matches(filter, @"[\#\w]+|""[\w\s]*""");
                string add = "";
                string add2 = "(";

                foreach (Match match in matches)
                {
                    if (match.Value.StartsWith("#"))
                    {
                        if (match.Value.Length > 1)
                        {
                            var remainder = match.Value.Substring(1, match.Length - 1).ToLowerInvariant();

                            switch (remainder)
                            {
                                case "known":
                                    add2 += string.Format("term.State={0} OR ", (int)TermState.Known);
                                    break;

                                case "unknown":
                                case "notknown":
                                    add2 += string.Format("term.State={0} OR ", (int)TermState.Unknown);
                                    break;

                                case "ignore":
                                case "ignored":
                                    add2 += string.Format("term.State={0} OR ", (int)TermState.Ignored);
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
                            add += string.Format("(term.Phrase LIKE '{0}%' OR term.BasePhrase LIKE '{0}%' OR Language LIKE '{0}%') AND ", value.Replace("'", "''"));
                        }
                    }
                }

                if (add2.Length > 1)
                {
                    sql.Append(" AND " + add2.Substring(0, add2.Length - 3) + ")");
                }
                if (add.Length > 0)
                {
                    sql.Append(" AND " + add.Substring(0, add.Length - 4));
                }
            }

            if (languageId.HasValue)
            {
                sql.Append("AND term.LanguageId=@0", languageId.Value);
            }

            if (modified.HasValue)
            {
                sql.Append("AND term.DateModified>=@0", modified.Value.ToUniversalTime());
            }

            sql.OrderBy("Language, term.State, term.LowerPhrase");

            if (maxResults.HasValue && maxResults > 0)
            {
                sql.Append("LIMIT @0", maxResults.Value);
            }

            var results = _db.Query<Term>(sql);
            return results;
        }

        public TermStatistics GetStatistics()
        {
            var log = _db.Fetch<TermLog>("WHERE UserId=@0", _user.UserId);
            TermStatistics stats = new TermStatistics();

            foreach (var t in log)
            {
                var date = new DateTime(t.EntryDate.Year, t.EntryDate.Month, t.EntryDate.Day);
                TermStatistic stat;

                if (!stats.Statistics.ContainsKey(date))
                {
                    stats.Statistics.Add(date, new TermStatistic());
                }

                stat = stats.Statistics[date];

                if (!stat.PerLanguage.ContainsKey(t.LanguageId))
                {
                    stat.PerLanguage.Add(t.LanguageId, new TermStatistic.Types());
                }

                var language = stat.PerLanguage[t.LanguageId];

                switch (t.Type)
                {
                    case TermType.Create:
                        language.Created[t.State]++;
                        break;

                    case TermType.Delete:
                        language.Deleted[t.State]++;
                        break;

                    case TermType.Modify:
                        language.Modified[t.State]++;
                        break;

                    case TermType.Unknown:
                        break;
                }

                stat.PerLanguage[t.LanguageId] = language;
            }

            return stats;
        }
    }
}
