using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Awesomium.Core;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Owin.Hosting;
using Ninject;
using Ninject.Activation;
using NPoco;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User User { get; set; }
        private static IKernel _container;

        public static IKernel Container
        {
            get { return _container; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _container = Setup.Start(_container);

            var skip = new Ninject.Parameters.ConstructorArgument("skip", true);
            var userDialog = Container.Get<UserDialog>(skip);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var result = userDialog.ShowDialog();
            ShutdownMode = ShutdownMode.OnLastWindowClose;

            if (result != true)
            {
                App.User = new User(); //TODO fixme
                this.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Setup.Shutdown(App.Container);
            WebCore.Shutdown();
            base.OnExit(e);
        }
    }
}
