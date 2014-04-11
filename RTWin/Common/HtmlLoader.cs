using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTWin.Common
{
    public sealed class HtmlLoader
    {
        private static readonly Lazy<HtmlLoader> _lazy = new Lazy<HtmlLoader>(() => new HtmlLoader());
        public static HtmlLoader Instance { get { return _lazy.Value; } }

        public string Html { get; private set; }
        private string _path;
        private FileSystemWatcher _watcher = new FileSystemWatcher();

        private HtmlLoader()
        {
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Reading.html");

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path));
            _watcher.Filter = Path.GetFileName(_path);
            _watcher.IncludeSubdirectories = false;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            _watcher.Changed += WatcherOnChanged;
            _watcher.EnableRaisingEvents = true;

            LoadHtml();
        }

        public void LoadHtml()
        {
            using (var sr = new StreamReader(_path, Encoding.UTF8))
            {
                Html = sr.ReadToEnd();
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            LoadHtml();
        }
    }
}
