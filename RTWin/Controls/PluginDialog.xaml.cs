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
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for PluginDialog.xaml
    /// </summary>
    public partial class PluginDialog : UserControl
    {
        private readonly PluginService _pluginService;
        public Plugin Plugin { get; set; }

        public PluginDialog(Plugin plugin)
        {
            _pluginService = App.Container.Get<PluginService>();
            InitializeComponent();

            Stream xshd_stream = File.OpenRead(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Javascript-Mode.xshd"));
            XmlTextReader xshd_reader = new XmlTextReader(xshd_stream);
            TextBoxContent.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            xshd_reader.Close();
            xshd_stream.Close();

            BindPlugin(plugin);
        }

        private void BindPlugin(Plugin plugin)
        {
            if (plugin == null)
            {
                plugin = new Plugin();
            }

            TextBoxContent.Text = plugin.Content;
            Plugin = plugin;
            this.DataContext = Plugin;
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Name = TextBoxName.Text;
            Plugin.Content = TextBoxContent.Text;
            Plugin.Description = TextBoxDescription.Text;

            _pluginService.Save(Plugin);
            BindPlugin(Plugin);
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var plugin = _pluginService.FindOne(Plugin.PluginId);
            BindPlugin(plugin);
        }
    }
}
