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
using System.Windows.Shapes;
using System.Xml;
using log4net.Plugin;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for PluginDialog.xaml
    /// </summary>
    public partial class PluginDialog : Window
    {
        private readonly PluginService _pluginService;

        public PluginDialog(PluginService pluginService)
        {
            _pluginService = pluginService;
            InitializeComponent();

            BindPlugins();

            Stream xshd_stream = File.OpenRead(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Javascript-Mode.xshd"));
            XmlTextReader xshd_reader = new XmlTextReader(xshd_stream);
            TextBoxContent.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            xshd_reader.Close();
            xshd_stream.Close();
        }

        private void BindPlugins()
        {
            ListBoxPlugins.ItemsSource = _pluginService.FindAll();
        }

        private void BindPlugin(Plugin plugin)
        {
            if (plugin == null)
            {
                Reset();
                return;
            }

            TextBoxName.Text = plugin.Name;
            TextBoxDescription.Text = plugin.Description;
            TextBoxContent.Text = plugin.Content;
        }

        private void Reset()
        {
            ListBoxPlugins.SelectedIndex = -1;
            TextBoxName.Text = "";
            TextBoxDescription.Text = "";
            TextBoxContent.Text = "";
        }

        private void ButtonNewPlugin_OnClick(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin plugin;

            if (ListBoxPlugins.SelectedIndex < 0)
            {
                plugin = new Plugin()
                {
                    Name = TextBoxName.Text,
                    Content = TextBoxContent.Text,
                    Description = TextBoxDescription.Text,
                    UUID = Guid.NewGuid().ToString()
                };
            }
            else
            {
                plugin = ListBoxPlugins.SelectedItem as Plugin;

                if (plugin == null)
                {
                    MessageBox.Show("Could not save changes to plugin", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                plugin.Name = TextBoxName.Text;
                plugin.Content = TextBoxContent.Text;
                plugin.Description = TextBoxDescription.Text;
            }

            _pluginService.Save(plugin);
            BindPlugins();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            int index = ListBoxPlugins.SelectedIndex;
            Reset();
            ListBoxPlugins.SelectedIndex = index;
        }

        private void ListBoxPlugins_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindPlugin(ListBoxPlugins.SelectedItem as Plugin);
        }

        private void ButtonDeletePlugin_OnClick(object sender, RoutedEventArgs e)
        {
            var plugin = ListBoxPlugins.SelectedItem as Plugin;

            if (plugin == null)
            {
                return;
            }

            _pluginService.DeleteOne(plugin.PluginId);
            BindPlugins();
        }
    }
}
