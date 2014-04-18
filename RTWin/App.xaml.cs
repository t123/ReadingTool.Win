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

            var userService = _container.Get<UserService>();

            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var users = userService.FindAll();
            if (users.Count() == 1)
            {
                App.User = users.First();
            }
            else
            {
                var userDialog = Container.Get<UserDialog>();
                var result = userDialog.ShowDialog();

                if (result != true)
                {
                    App.User = new User(); //TODO fixme
                    this.Shutdown();
                    return;
                }
            }

            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            var mainWindow = _container.Get<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Setup.Shutdown(App.Container);
            WebCore.Shutdown();
            base.OnExit(e);
        }
    }
}
