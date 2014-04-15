﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dependencies;
using System.Windows;
using Newtonsoft.Json;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Web
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TermsController : ApiController
    {
        private TermService _termService;
        private ItemService _itemService;

        public class TermReponse
        {
            public long Id { get; set; }
            public string Phrase { get; set; }
            public string BasePhrase { get; set; }
            public string Definition { get; set; }
            public string State { get; set; }
            public string Sentence { get; set; }
        }

        public TermsController()
        {
            _termService = App.Container.Get<TermService>();
            _itemService = App.Container.Get<ItemService>();
        }

        public TermReponse Get(long id)
        {
            var term = _termService.FindOne(id);

            if (term == null)
            {
                return null;
            }

            return new TermReponse()
            {
                BasePhrase = term.BasePhrase,
                State = term.State.ToString().ToLowerInvariant(),
                Phrase = term.Phrase,
                Id = term.TermId,
                Definition = term.Definition,
                Sentence = term.Sentence
            };
        }

        public async Task<TermReponse> GetTerm(string phrase = "", int languageId = 0)
        {
            if (string.IsNullOrWhiteSpace(phrase) || languageId <= 0)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Term term = null;
            await Task.Run(() =>
            {
                term = _termService.FindOneByPhraseAndLanguage(phrase, languageId);
            });

            if (term == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return new TermReponse()
            {
                BasePhrase = term.BasePhrase,
                State = term.State.ToString().ToLowerInvariant(),
                Phrase = term.Phrase,
                Id = term.TermId,
                Definition = term.Definition,
                Sentence = term.Sentence
            };
        }

        public HttpResponseMessage Delete(TermPost term)
        {
            var exists = _termService.FindOneByPhraseAndLanguage(term.Phrase, term.LanguageId);

            if (exists == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            _termService.DeleteOne(exists.TermId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public HttpResponseMessage Post(TermPost term)
        {
            var exists = _termService.FindOneByPhraseAndLanguage(term.Phrase, term.LanguageId);

            if (exists == null)
            {
                var t = new Term
                {
                    BasePhrase = (term.BasePhrase ?? "").Trim(),
                    State = term.State,
                    Phrase = term.Phrase,
                    Definition = (term.Definition ?? "").Trim(),
                    Sentence = (term.Sentence ?? "").Trim(),
                    ItemSourceId = term.ItemId,
                    LanguageId = term.LanguageId,
                };

                _termService.Save(t);

                var response = new HttpResponseMessage(HttpStatusCode.Created);
                return response;
            }
            else
            {
                exists.BasePhrase = (term.BasePhrase ?? "").Trim();
                exists.Definition = (term.Definition ?? "").Trim();
                exists.State = term.State;

                if (string.IsNullOrWhiteSpace(exists.Sentence))
                {
                    exists.Sentence = term.Sentence.Trim();
                    exists.ItemSourceId = term.ItemId;
                }
                else if (string.Compare(exists.Sentence, term.Sentence, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    exists.Sentence = term.Sentence.Trim();
                    exists.ItemSourceId = term.ItemId;
                }

                _termService.Save(exists);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        [HttpPost]
        [Route("api/terms/markasread")]
        public HttpResponseMessage MarkAllAsRead(TermPost[] terms)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            if (terms == null)
            {
                response.Content = new StringContent("0");
                return response;
            }

            int counter = 0;
            foreach (var term in terms)
            {
                try
                {
                    var t = new Term()
                    {
                        BasePhrase = "",
                        Definition = "",
                        Sentence = "",
                        Phrase = term.Phrase,
                        State = TermState.Known,
                        LanguageId = term.LanguageId,
                        ItemSourceId = term.ItemId,
                    };

                    _termService.Save(t);
                    counter++;
                }
                catch (SQLiteException exception)
                {
                    //Just ignore them
                }
            }

            response.Content = new StringContent(counter.ToString());
            return response;
        }

        public class ChangeRead
        {
            public int Amount { get; set; }
            public string Type { get; set; }
            public long ItemId { get; set; }
        }

        [HttpPost]
        [Route("api/terms/updatecount")]
        public HttpResponseMessage Update(ChangeRead change)
        {
            var item = _itemService.FindOne(change.ItemId);

            if (item == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var message = _itemService.ChangeStatistics(item, change.Type, change.Amount);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(message));
            return response;
        }

        public class TermPost
        {
            public string Phrase { get; set; }
            public string BasePhrase { get; set; }
            public string Sentence { get; set; }
            public string Definition { get; set; }
            public long LanguageId { get; set; }
            public long ItemId { get; set; }
            public TermState State { get; set; }
        }
    }
}
