using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Ninject;
using RTWin.Services;

namespace RTWin.Web
{
    [RoutePrefix("api/resource")]
    public class ResourceController : ApiController
    {
        [HttpGet]
        [Route("plugins/{id}")]
        public HttpResponseMessage GetPlugins(long id)
        {
            var pluginService = App.Container.Get<PluginService>();
            var plugins = pluginService.FindAllForLanguage(id);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("//Generated at {0}", DateTime.Now));

            if (plugins.Any())
            {
                sb.AppendLine("$(document).on('pluginReady', function() {");

                foreach (var plugin in plugins)
                {
                    sb.AppendLine("");
                    sb.AppendLine("/*");
                    sb.AppendLine("* " + plugin.Name);
                    sb.AppendLine("* " + plugin.UUID);
                    sb.AppendLine("* " + plugin.Description);
                    sb.AppendLine("*/");
                    sb.AppendLine(plugin.Content);
                    sb.AppendLine("");
                }

                sb.AppendLine("$(document).trigger('pluginInit');");
                sb.AppendLine("});");
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(sb.ToString());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");

            return response;
        }

        [HttpGet]
        [Route("media/{id}")]
        public HttpResponseMessage GetMedia(long id)
        {
            try
            {
                var itemService = App.Container.Get<ItemService>();
                var item = itemService.FindOne(id);

                if (item == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }


                var fileStream = new FileStream(item.MediaUri, FileMode.Open, FileAccess.Read);
                MediaTypeHeaderValue header = null;
                switch (Path.GetExtension(item.MediaUri))
                {
                    case ".mp3":
                        header = new MediaTypeHeaderValue("audio/mpeg3");
                        break;

                    case ".mp4":
                        header = new MediaTypeHeaderValue("video/mp4");
                        break;
                }

                if (Request.Headers.Range != null)
                {
                    try
                    {
                        HttpResponseMessage partialResponse = Request.CreateResponse(HttpStatusCode.PartialContent);
                        partialResponse.Content = new ByteRangeStreamContent(fileStream, Request.Headers.Range, header);
                        return partialResponse;
                    }
                    catch (InvalidByteRangeException invalidByteRangeException)
                    {
                        return Request.CreateErrorResponse(invalidByteRangeException);
                    }
                }
                else
                {
                    HttpResponseMessage fullResponse = Request.CreateResponse(HttpStatusCode.OK);
                    fullResponse.Content = new StreamContent(fileStream);
                    fullResponse.Content.Headers.ContentType = header;
                    return fullResponse;
                }
            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("item/{id}")]
        public HttpResponseMessage GetItem(long id)
        {
            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", id + ".html");

            if (!File.Exists(filename))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(filename, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }

        [HttpGet]
        [Route("local/{id}")]
        public HttpResponseMessage GetResource(string id)
        {
            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Resources", id);

            if (!File.Exists(filename))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(filename, FileMode.Open, FileAccess.Read));

            switch (Path.GetExtension(id))
            {
                case ".js":
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
                    break;

                case ".css":
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/css");
                    break;

                case ".swf":
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-shockwave-flash");
                    break;

                case ".jpg":
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    break;

                case ".png":
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    break;

                default:
                    break;
            }

            return response;
        }
    }
}
