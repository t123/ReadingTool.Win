using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RTWin.Annotations;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class PluginsControlViewModel : BaseViewModel
    {
        private readonly PluginService _pluginService;
        private ObservableCollection<PluginModel> _plugins;
        private PluginModel _selectedItem;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private ICommand _exportCommand;

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

        public PluginModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<PluginModel> Plugins
        {
            get
            {
                if (_plugins == null)
                {
                    _plugins = new ObservableCollection<PluginModel>(Mapper.Map<IEnumerable<Plugin>, IEnumerable<PluginModel>>(_pluginService.FindAll()));
                }

                return _plugins;
            }
            set
            {
                _plugins = value;
                OnPropertyChanged("Plugins");
            }
        }

        public PluginsControlViewModel(PluginService pluginService)
        {
            _pluginService = pluginService;
            SelectedItem = Plugins.FirstOrDefault();

            _addCommand = new RelayCommand(() =>
            {
                var plugin = Plugin.NewPlugin();
                _pluginService.Save(plugin);
                var mapped = Mapper.Map<Plugin, PluginModel>(plugin);
                Plugins.Add(mapped);
                SelectedItem = mapped;
            }
               );

            _deleteCommand = new RelayCommand(() =>
            {
                var result = MessageBox.Show(
                    string.Format("Are you sure you want to delete {0}?", SelectedItem.Name),
                    string.Format("Delete {0}", SelectedItem.Name),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation
                    );

                if (result == MessageBoxResult.Yes)
                {
                    _pluginService.DeleteOne(SelectedItem.PluginId);
                    Plugins.Remove(SelectedItem);
                    SelectedItem = Plugins.FirstOrDefault();
                }
            }, () => SelectedItem != null);

            _saveCommand = new RelayCommand(() =>
            {
                var mapped = SelectedItem.ToPlugin();
                _pluginService.Save(mapped);
            }, () => SelectedItem != null && SelectedItem.IsValid);

            _exportCommand = new RelayCommand(() =>
            {
                string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                foreach (var plugin in _pluginService.FindAll())
                {
                    string filename = string.Format("{0}.js", plugin.Name);
                    foreach (var c in Path.GetInvalidFileNameChars())
                    {
                        filename = filename.Replace(c, '_');
                    }

                    var output = Path.Combine(outputPath, filename);
                    //TODO dont clobber files

                    using (StreamWriter sw = new StreamWriter(output, false, Encoding.UTF8))
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
        }
    }
}
