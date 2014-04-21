using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class PluginsControlViewModel : INotifyPropertyChanged
    {
        private readonly PluginService _pluginService;
        private ObservableCollection<Plugin> _plugins;
        private Plugin _selectedItem;
        private Plugin _plugin;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private ICommand _exportCommand;
        private ICommand _backCommand;

        public ICommand BackCommand
        {
            get { return _backCommand; }
            set { _backCommand = value; }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
            set { _saveCommand = value; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
            set { _cancelCommand = value; }
        }

        public ICommand AddCommand
        {
            get { return _addCommand; }
            set { _addCommand = value; }
        }

        public ICommand DeleteCommand
        {
            get { return _deleteCommand; }
            set { _deleteCommand = value; }
        }

        public ICommand ExportCommand
        {
            get { return _exportCommand; }
            set { _exportCommand = value; }
        }

        public Plugin Plugin
        {
            get { return _plugin; }
            set { _plugin = value; OnPropertyChanged("Plugin"); }
        }

        public Plugin SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    Plugin = _pluginService.FindOne(_selectedItem.PluginId);
                }

                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<Plugin> Plugins
        {
            get { return _plugins; }
            set { _plugins = value; OnPropertyChanged("Plugins"); }
        }

        public PluginsControlViewModel(PluginService pluginService)
        {
            _pluginService = pluginService;

            //TODO fixme
            //Stream xshd_stream = File.OpenRead(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Javascript-Mode.xshd"));
            //XmlTextReader xshd_reader = new XmlTextReader(xshd_stream);
            //TextBoxContent.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            //xshd_reader.Close();
            //xshd_stream.Close();

            _plugins = new ObservableCollection<Plugin>(_pluginService.FindAll());
            SelectedItem = Plugins.FirstOrDefault();

            _addCommand = new RelayCommand(async param =>
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                var result = await metroWindow.ShowInputAsync("Please enter a name for your plugin", "Plugin Name", MainWindowViewModel.DialogSettings);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var plugin = new Plugin()
                    {
                        Name = result.Trim(),
                        UUID = Guid.NewGuid().ToString(),
                        Content = "",
                        Description = "",
                    };

                    _pluginService.Save(plugin);
                    Plugins = new ObservableCollection<Plugin>(_pluginService.FindAll());
                    SelectedItem = Plugins.FirstOrDefault(x => x.PluginId == plugin.PluginId);
                }
            });

            _deleteCommand = new RelayCommand(async param =>
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                var result = await metroWindow.ShowMessageAsync("Delete " + SelectedItem.Name, "Are you sure you want to delete " + SelectedItem.Name + "?", MessageDialogStyle.AffirmativeAndNegative, MainWindowViewModel.DialogSettings);

                if (result == MessageDialogResult.Negative)
                    return;

                _pluginService.DeleteOne(SelectedItem.PluginId);
                Plugins = new ObservableCollection<Plugin>(_pluginService.FindAll());
                SelectedItem = Plugins.FirstOrDefault();
            }, param => SelectedItem != null);

            _saveCommand = new RelayCommand(param =>
            {
                var plugin = _pluginService.FindOne(SelectedItem.PluginId);

                plugin.Name = Plugin.Name;
                plugin.Content = Plugin.Content;
                plugin.Description = Plugin.Description;

                _pluginService.Save(plugin);
                Plugins = new ObservableCollection<Plugin>(_pluginService.FindAll());
                SelectedItem = Plugins.FirstOrDefault(x => x.PluginId == plugin.PluginId);
            }, param => SelectedItem != null);

            _cancelCommand = new RelayCommand(param =>
            {
                Plugin = _pluginService.FindOne(SelectedItem.PluginId);
                SelectedItem = Plugins.FirstOrDefault(x => x.PluginId == Plugin.PluginId);
            }, param => SelectedItem != null);

            _exportCommand = new RelayCommand(param =>
            {
                string outputPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                foreach (var plugin in _pluginService.FindAll())
                {
                    string filename = string.Format("{0}.js", plugin.UUID);

                    using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(outputPath, filename), false, Encoding.UTF8))
                    {
                        sw.WriteLine("/*");
                        sw.WriteLine(plugin.UUID.Replace("*/", "* /"));
                        sw.WriteLine(plugin.Name.Replace("*/", "* /"));
                        sw.WriteLine(plugin.Description.Replace("*/", "* /"));
                        sw.WriteLine("*/");
                        sw.WriteLine(plugin.Content);
                    }
                }
            });

            _backCommand = new RelayCommand(param => Messenger.Default.Send<ChangeViewMessage>(new ChangeViewMessage(ChangeViewMessage.Main)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
