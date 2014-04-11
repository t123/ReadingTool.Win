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
    public class MediaController : ApiController
    {
        public HttpResponseMessage Get(long id)
        {
            try
            {
                var itemService = App.Container.Get<ItemService>();
                var item = itemService.FindOne(id);

                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new FileStream(item.MediaUri, FileMode.Open, FileAccess.Read));

                switch (Path.GetExtension(item.MediaUri))
                {
                    case ".mp3":
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg3");
                        break;

                    case ".mp4":
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("video/mpeg");
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
    }
}
