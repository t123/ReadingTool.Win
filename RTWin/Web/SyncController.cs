using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Ninject;
using RTWin.Entities;
using RTWin.Entities.Enums;
using RTWin.Services;

namespace RTWin.Web
{
    [RoutePrefix("api/v1")]
    public class SyncController : ApiController
    {
        private TermService _termService;
        private ItemService _itemService;
        private LanguageService _languageService;

        public SyncController()
        {
            _termService = App.Container.Get<TermService>();
            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();
        }

        #region terms
        [HttpGet]
        [Route("terms/get")]
        public IEnumerable<Term> GetTerms(long? languageId = null, DateTime? modified = null)
        {
            return _termService.Search(languageId, modified);
        }

        [HttpGet]
        [Route("terms/get/{id}")]
        public Term GetTerm(long id)
        {
            return _termService.FindOne(id);
        }

        [HttpDelete]
        [Route("terms/delete/{id}")]
        public HttpResponseMessage DeleteTerm(long id)
        {
            var exists = _termService.FindOne(id);

            if (exists == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            _termService.DeleteOne(exists.TermId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("terms/statistics")]
        public TermStatistics GetTermStatistics()
        {
            return _termService.GetStatistics();
        }
        #endregion

        #region items
        [HttpGet]
        [Route("items/get")]
        public IEnumerable<Item> GetItems(
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
            return _itemService.Search(itemType, modified, collectionName, title, languageId, isParallel, hasMedia, filter, maxResults);
        }

        [HttpGet]
        [Route("items/get/{id}")]
        public Item GetItem(long id)
        {
            return _itemService.FindOne(id);
        }

        [HttpDelete]
        [Route("items/delete/{id}")]
        public HttpResponseMessage DeleteItem(long id)
        {
            var exists = _itemService.FindOne(id);

            if (exists == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            _itemService.DeleteOne(exists.ItemId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        #endregion

        #region languages
        [HttpGet]
        [Route("languages/get")]
        public IEnumerable<Language> GetLanguages()
        {
            return _languageService.FindAll();
        }

        [HttpGet]
        [Route("languages/get/{id}")]
        public Language GetLanguage(long id)
        {
            return _languageService.FindOne(id);
        }

        [HttpDelete]
        [Route("languages/delete/{id}")]
        public HttpResponseMessage DeleteLanguage(long id)
        {
            var exists = _languageService.FindOne(id);

            if (exists == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            _languageService.DeleteOne(exists.LanguageId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        #endregion
    }
}
