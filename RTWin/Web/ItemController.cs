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
    public class ItemController : ApiController
    {
        public HttpResponseMessage Get(long id)
        {
            var filename = Path.Combine(@"C:\gitrepository\RTWin\RTWin\bin\Debug\Data", id + ".html");
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(filename, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }
    }
}
