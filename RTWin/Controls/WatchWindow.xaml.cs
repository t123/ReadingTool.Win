using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    /// Interaction logic for WatchWindow.xaml
    /// </summary>
    public partial class WatchWindow : Window
    {
        private Item _item;
        private ParserOutput _output;
        private HubConnection hubConnection;
        private IHubProxy mainHubProxy;
        private int _lastL1 = -1;
        private int _lastL2 = -1;
        private TermService _termService;
        private LanguageService _languageService;
        private VideoParserService _vps;

        public WatchWindow(Item item)
        {
            _item = item;
            _languageService = App.Container.Get<LanguageService>();
            _termService = App.Container.Get<TermService>();

            InitializeComponent();

            BindItem();
        }

        public Tuple<long, long> GetSub(double time)
        {
            var l1Sub = _vps.L1Srt.FirstOrDefault(x => x.Start < time && x.End > time);
            var l2Sub = _vps.L2Srt.FirstOrDefault(x => x.Start < time && x.End > time);

            var l1 = l1Sub == null ? -1 : l1Sub.LineNo;
            var l2 = l1Sub == null ? -1 : l2Sub.LineNo;

            return new Tuple<long, long>(l1, l2);
        }

        private void BindItem()
        {
            ParserInput pi = new ParserInput()
                .WithItem(_item)
                .IsParallel()
                .WithLanguage1(_languageService.FindOne(_item.L1LanguageId))
                .WithLanguage2(_languageService.FindOne(_item.L2LanguageId))
                .WithTerms(_termService.FindAllByLanguage(_item.L1LanguageId))
                .WithHtml(HtmlLoader.Instance.WatchingHtml)
                ;

            _vps = new VideoParserService(pi);
            _output = _vps.Parse();

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
                    EnableGPUAcceleration = true,
                    WebSecurity = false,
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

        private void WebControl_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
