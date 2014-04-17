using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Web
{
    [RoutePrefix("api/v1/plugin")]
    public class PluginController : ApiController
    {
        private PluginService _pluginService;

        public class PluginGet
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class PluginStore
        {
            public Guid Uuid { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class PluginDelete
        {
            public Guid Uuid { get; set; }
            public string Key { get; set; }
        }

        public PluginController()
        {
            _pluginService = App.Container.Get<PluginService>();
        }

        [Route("store")]
        [HttpPost]
        public HttpResponseMessage Store(PluginStore data)
        {
            var storage = _pluginService.Get(data.Uuid, data.Key);

            if (storage == null)
            {
                storage = new PluginStorage()
                {
                    Key = data.Key,
                    UUID = data.Uuid.ToString(),
                    Value = data.Value
                };

                _pluginService.Store(storage);
                return new HttpResponseMessage(HttpStatusCode.Created);
            }

            storage.Value = data.Value;
            _pluginService.Store(storage);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [Route("get/{uuid}")]
        [HttpGet]
        public HttpResponseMessage Get(Guid uuid, string key = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            if (string.IsNullOrWhiteSpace(key))
            {
                var storage = _pluginService.Get(uuid).Select(x => new PluginGet() { Key = x.Key, Value = x.Value }).ToArray();
                response.Content = new StringContent(JsonConvert.SerializeObject(storage));
            }
            else
            {
                var storage = _pluginService.Get(uuid, key);

                if (storage == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                response.Content = new StringContent(JsonConvert.SerializeObject(new PluginGet() { Key = key, Value = storage.Value }));
            }

            return response;
        }

        [Route("remove/{uuid}")]
        [HttpDelete]
        public HttpResponseMessage Delete(Guid uuid, string key = null)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _pluginService.Remove(uuid, key);

            return response;
        }
    }
}
