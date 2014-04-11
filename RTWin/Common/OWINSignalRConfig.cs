using Microsoft.Owin.Cors;
using Owin;

namespace RTWin.Common
{
    public class OWINSignalRConfig
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.MapSignalR();
        }
    }
}