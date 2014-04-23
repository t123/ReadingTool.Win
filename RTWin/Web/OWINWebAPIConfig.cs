using System.Web.Http;
using System.Web.Http.Cors;
using Owin;

namespace RTWin.Web
{
    public class OWINWebAPIConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new System.Web.Http.HttpConfiguration();

            config.MapHttpAttributeRoutes();
            var enableCorsAttribute = new EnableCorsAttribute("*", "Origin, Content-Type, Accept", "GET, PUT, POST, DELETE, OPTIONS");
            config.EnableCors(enableCorsAttribute);

            appBuilder.UseWebApi(config);
        }
    }
}