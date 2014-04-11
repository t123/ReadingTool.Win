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
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = Path.GetFileName(item.MediaUri);

                return response;
            }
            catch (Exception e)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
