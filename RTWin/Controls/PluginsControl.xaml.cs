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
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for PluginsControl.xaml
    /// </summary>
    public partial class PluginsControl : UserControl
    {
        private readonly PluginsControlViewModel _pluginsControlViewModel;

        public PluginsControl(PluginsControlViewModel pluginsControlViewModel)
        {
            _pluginsControlViewModel = pluginsControlViewModel;
            InitializeComponent();
            this.DataContext = _pluginsControlViewModel;
        }
    }
}
