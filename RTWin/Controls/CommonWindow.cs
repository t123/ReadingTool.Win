using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Ninject;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Entities.Enums;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    public class CommonWindow
    {
        protected Item _item;
        protected bool _parallel;
        protected ParserOutput _output;
        protected TermService _termService;
        protected LanguageService _languageService;
        protected VideoParserService _vps;
        protected ItemService _itemService;
        protected PluginService _pluginService;
        protected DatabaseService _databaseService;
        protected IParserService _parserService;
        protected WebControl _control;
        private ProgressDialogController _progress;

        public ParserOutput Output
        {
            get { return _output; }
        }

        public CommonWindow(WebControl control)
        {
            _control = control;

            InitializeWebControl(_control);
        }

        public async void Read(Item item, bool parallel)
        {
            var metroWindow = (Application.Current.MainWindow as MetroWindow);
            _progress = await metroWindow.ShowProgressAsync("Please wait...", "Your material will be available in a few seconds.", false, MainWindowViewModel.DialogSettings);

            _item = item;
            _parallel = parallel;
            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();
            _termService = App.Container.Get<TermService>();
            _pluginService = App.Container.Get<PluginService>();
            _databaseService = App.Container.Get<DatabaseService>();
            //_parserService = parserService;

            _parserService = item.ItemType == ItemType.Text ? (IParserService)new ParserService() : (IParserService)new VideoParserService();
            var html = item.ItemType == ItemType.Text ? HtmlLoader.Instance.ReadingHtml : HtmlLoader.Instance.WatchingHtml;

            var signalR = _databaseService.GetSetting<string>(DbSetting.Keys.BaseWebSignalRAddress);
            var webApi = _databaseService.GetSetting<string>(DbSetting.Keys.BaseWebAPIAddress);
            ParserInput pi = new ParserInput()
                .WithItem(_item)
                .IsParallel(_parallel)
                .WithLanguage1(_languageService.FindOne(_item.L1LanguageId))
                .WithLanguage2(_languageService.FindOne(_item.L2LanguageId ?? 0))
                .WithTerms(_termService.FindAllByLanguage(_item.L1LanguageId))
                .WithHtml(html)
                .WithSignalREndPoint(signalR)
                .WithWebApiEndPoint(webApi)
                ;

            _output = _parserService.Parse(pi);
            _output = InjectPlugins(_output);
            _output.Html = _output.Html.Replace("<!-- webapi -->", webApi);
            _output.Html = _output.Html.Replace("<!-- signalr -->", signalR);

            WriteHtml(_output.Html, ".html");
            WriteHtml(_output.Xml, ".xml");
            MarkAsRead();

            var sourceUri = _databaseService.GetSetting<string>(DbSetting.Keys.BaseWebAPIAddress) + "/api/resource/item/" + _item.ItemId;
            //TODO fixme, navigate away sourceurl doesn't change if view again, dialog doesn't close.
            _control.Source = "about:blank".ToUri();
            _control.Source = sourceUri.ToUri();
        }

        public string GetTitle()
        {
            return (_item.CollectionNo == null ? "" : (_item.CollectionNo.ToString() + ". ")) +
                    (string.IsNullOrWhiteSpace(_item.CollectionName) ? "" : (_item.CollectionName + " - ")) +
                    _item.L1Title
                    ;
        }

        private void MarkAsRead()
        {
            _itemService.MarkLastRead(_item.ItemId);
        }

        private ParserOutput InjectPlugins(ParserOutput po)
        {
            po.Html = _output.Html.Replace("<!-- plugins -->", string.Format(@"<script src=""<!-- webapi -->/api/resource/plugins/{0}""></script>", _item.L1LanguageId));
            return po;
        }

        private void WriteHtml(string html, string extension)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", _item.ItemId + extension), false, Encoding.UTF8))
            {
                sw.Write(html);
            }
        }

        private void InitializeWebControl(WebControl control)
        {
            _control = control;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "awesomium");

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
                    WebSecurity = false,
                    CanScriptsAccessClipboard = true,
                    Databases = true,
                    LoadImagesAutomatically = true,
                    EnableGPUAcceleration = true,
                    RemoteFonts = true,
                    LocalStorage = true,
                    Javascript = true,
                    AppCache = true
                }
                    );
            }

            //var sourceUri = _databaseService.GetSetting<string>(DbSetting.Keys.BaseWebAPIAddress) + "/api/resource/item/" + _item.ItemId;
            var sourceUri = "about:blank";
            control.WebSession = session;
            control.ConsoleMessage += WebControl_ConsoleMessage;
            control.DocumentReady += WebControl_OnDocumentReady;
            control.ShowCreatedWebView += WebControl_ShowCreatedWebView;
            control.Source = sourceUri.ToUri();

            control.LoadingFrameComplete += (s, e) =>
            {
                if (e.IsMainFrame && _progress != null)
                {
                    _progress.CloseAsync();
                }
            };
        }

        void WebControl_ShowCreatedWebView(object sender, ShowCreatedWebViewEventArgs e)
        {
            var target = e.TargetURL;
            Process.Start(target.ToString());
            e.Cancel = true;
        }

        protected void WebControl_OnDocumentReady(object sender, UrlEventArgs e)
        {
            JSObject jsObject = _control.CreateGlobalJavascriptObject("rtjscript");
            jsObject.Bind("copyToClipboard", false, JSHandler);
        }

        protected void JSHandler(object sender, JavascriptMethodEventArgs args)
        {
            if (args.MethodName == "copyToClipboard" && args.Arguments.Length > 0)
            {
                string value = args.Arguments[0];

                if (!string.IsNullOrWhiteSpace(value))
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Clipboard.SetText(value, TextDataFormat.UnicodeText));
                }
            }
        }

        protected void WebControl_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            return;
        }
    }
}
