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

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new FileStream(item.MediaUri, FileMode.Open, FileAccess.Read));

                switch (Path.GetExtension(item.MediaUri))
                {
                    case ".mp3":
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg3");
                        break;

                    case ".mp4":
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                        break;

                    default:
                        break;
                }

                return response;
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
