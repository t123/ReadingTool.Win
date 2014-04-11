using System.Web.Http;
using Owin;

namespace RTWin.Common
{
    public class OWINWebAPIConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new System.Web.Http.HttpConfiguration();
            config.EnableCors();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            appBuilder.UseWebApi(config);
        }
    }
}