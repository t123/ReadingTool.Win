using System.Web.Http;
using System.Web.Http.Cors;
using Owin;

namespace RTWin.Common
{
    public class OWINWebAPIConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new System.Web.Http.HttpConfiguration();

            //config.MessageHandlers.Add(new CorsMessageHandler(config));
            //config.EnableCors();
            config.MapHttpAttributeRoutes();
            var enableCorsAttribute = new EnableCorsAttribute("*", "Origin, Content-Type, Accept", "GET, PUT, POST, DELETE, OPTIONS");
            config.EnableCors(enableCorsAttribute);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
                );

            appBuilder.UseWebApi(config);
        }
    }
}