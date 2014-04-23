using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RTWin.Core
{
    public sealed class ContentLoader
    {
        /// <summary>
        /// reading.html - HTML for reading
        /// </summary>
        public const string READING = "reading.html";
        /// <summary>
        /// watching.html - HTML for watching
        /// </summary>
        public const string WATCHING = "watching.html";
        /// <summary>
        /// video.xslt - XSLT for video transform
        /// </summary>
        public const string VIDEO = "video.xslt";
        /// <summary>
        /// text.xslt - XSLT for text transform
        /// </summary>
        public const string TEXT = "text.xslt";

        private static readonly Lazy<ContentLoader> _lazy = new Lazy<ContentLoader>(() => new ContentLoader());
        public static ContentLoader Instance { get { return _lazy.Value; } }

        private Dictionary<string, string> _content;
        private string _path;
        private FileSystemWatcher _watcher = new FileSystemWatcher();

        private ContentLoader()
        {
            _content = new Dictionary<string, string>(10);
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path));
            _watcher.Filter = "*.*";
            _watcher.IncludeSubdirectories = false;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            _watcher.Changed += WatcherOnChanged;
            _watcher.EnableRaisingEvents = true;

            Load();
        }

        public string Get(string filename)
        {
            if (!_content.ContainsKey(filename))
            {
                Load(filename);
            }

            return _content[filename];
        }

        private void Load(string filename = null)
        {
            if (filename == null)
            {
                _content[READING] = Read(READING);
                _content[WATCHING] = Read(WATCHING);
                _content[VIDEO] = Read(VIDEO);
                _content[TEXT] = Read(TEXT);
            }
            else
            {
                _content[filename] = Read(filename);
            }
        }

        private string Read(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var extension = Path.GetExtension(fileSystemEventArgs.Name);

            if (
                !string.IsNullOrWhiteSpace(extension) &&
                    (
                        extension.Equals(".html", StringComparison.InvariantCultureIgnoreCase) ||
                        extension.Equals(".xslt", StringComparison.InvariantCultureIgnoreCase)
                    )
                )
            {
                Load();
            }
        }
    }
}
