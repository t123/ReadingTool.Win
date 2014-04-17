using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for PluginsControl.xaml
    /// </summary>
    public partial class PluginsControl : UserControl
    {
        private PluginService _pluginService;

        public IList<Plugin> Plugins { get; set; }


        public PluginsControl()
        {
            _pluginService = App.Container.Get<PluginService>();
            InitializeComponent();

            BindPlugins();
        }

        private void BindPlugins()
        {
            Plugins = _pluginService.FindAll();
            this.DataContext = Plugins;

            if (ListBoxPlugins.Items.Count > 0)
            {
                ListBoxPlugins.SelectedIndex = 0;
            }
            else
            {
                ContentControl.Content = "";
            }
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button == null)
            {
                return;
            }

            if (button.Tag.ToString() == "Add")
            {
                PromptDialog promptDialog = new PromptDialog("Plugin Name", "Name ");
                var result = promptDialog.ShowDialog();

                if (result == true)
                {
                    var plugin = new Plugin()
                    {
                        Name = promptDialog.Input,
                        UUID = Guid.NewGuid().ToString(),
                        Content = "",
                        Description = "",
                    };

                    _pluginService.Save(plugin);

                    BindPlugins();
                }

                return;
            }
            else if (button.Tag.ToString() == "Export")
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
                return;
            }

            var item = ListBoxPlugins.SelectedItem as Plugin;

            if (item == null)
            {
                return;
            }

            if (button.Tag.ToString() == "Delete")
            {
                _pluginService.DeleteOne(item.PluginId);
                BindPlugins();
            }
        }

        private void ListBoxPlugins_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var plugin = ListBoxPlugins.SelectedItem as Plugin;

            if (plugin == null)
            {
                return;
            }

            ContentControl.Content = new PluginDialog(plugin);
        }
    }
}
