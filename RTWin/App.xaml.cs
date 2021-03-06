﻿using System;
using System.ComponentModel.Design;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Awesomium.Core;
using log4net;
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
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IKernel Container
        {
            get { return _container; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();

            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception exception = (Exception)args.ExceptionObject;
                Log.Error(exception);
            };

            try
            {
                _container = Setup.InitContainer(_container);
                Setup.Start();

                var userService = _container.Get<UserService>();
                App.User = userService.FindAll().OrderByDescending(x => x.LastLogin).First();

                var mainWindow = _container.Get<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception exception)
            {
                Setup.Instance.AddError("General startup", new Setup.Error("An unknown startup error has occurred", exception));
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Setup.Shutdown();
            WebCore.Shutdown();
            base.OnExit(e);
        }
    }
}
