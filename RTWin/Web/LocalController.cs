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

namespace RTWin.Web
{
    public class LocalController : ApiController
    {
        public HttpResponseMessage Get(string id)
        {
            var filename = Path.Combine(@"C:\gitrepository\RTWin\RTWin\bin\Debug\Data", id);
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

                default:
                    break;
            }
            

            return response;
        }
    }
}
