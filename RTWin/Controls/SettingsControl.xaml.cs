using System;
using System.Collections.Generic;
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
using RTWin.Models.Views;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private readonly SettingsControlViewModel _settingsControlViewModel;

        public SettingsControl(SettingsControlViewModel settingsControlViewModel)
        {
            _settingsControlViewModel = settingsControlViewModel;
            InitializeComponent();
            DataContext = _settingsControlViewModel;
        }
    }
}
