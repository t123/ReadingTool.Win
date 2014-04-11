﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Awesomium.Core;
using Awesomium.Core.Data;
using Microsoft.AspNet.SignalR.Client;
using Ninject;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Services;
using MediaState = System.Windows.Controls.MediaState;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ReadWindow.xaml
    /// </summary>
    public partial class ReadWindow : Window
    {
        protected Item _item;
        private ParserOutput _output;
        private LanguageService _languageService;
        private TermService _termService;

        public ReadWindow(Item item)
        {
            _item = item;
            _languageService = App.Container.Get<LanguageService>();
            _termService = App.Container.Get<TermService>();

            InitializeComponent();

            BindItem();
        }

        private void BindItem()
        {
            ParserInput pi = new ParserInput()
                .WithItem(_item)
                .IsParallel(false)
                .WithLanguage1(_languageService.FindOne(_item.L1LanguageId))
                .WithLanguage2(_languageService.FindOne(_item.L2LanguageId))
                .WithTerms(_termService.FindAllByLanguage(_item.L1LanguageId))
                .WithHtml(HtmlLoader.Instance.ReadingHtml)
                ;

            var ps = new ParserService(pi);
            _output = ps.Parse();

            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "awesomium"
                );

            var session = WebCore.Sessions.FirstOrDefault(x => x.DataPath == path);

            if (session == null)
            {
                session = WebCore.CreateWebSession(path, new WebPreferences()
                        {
                            SmoothScrolling = true,
                            FileAccessFromFileURL = true,
                            UniversalAccessFromFileURL = true,
                            DefaultEncoding = "utf-8",
                            AllowInsecureContent = true,
                            Plugins = true,
                            WebAudio = true,
                            WebGL = true,
                        }
                    );
            }

            WebControl.WebSession = session;
            WebControl.ConsoleMessage += WebControl_ConsoleMessage;
            WebControl.Source = ("http://localhost:9000/api/item/" + _item.ItemId).ToUri();

            Title = (_item.CollectionNo == null ? "" : (_item.CollectionNo.ToString() + ". ")) +
                    (string.IsNullOrWhiteSpace(_item.CollectionName) ? "" : (_item.CollectionName + " - ")) +
                    _item.L1Title
                    ;
        }

        void WebControl_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            return;
        }
    }
}
