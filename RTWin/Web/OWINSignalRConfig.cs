using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;

namespace RTWin.Web
{
    public class OWINSignalRConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.MapSignalR(new HubConfiguration()
            {
                EnableJSONP = true,
                EnableDetailedErrors = true
            });
        }
    }
}