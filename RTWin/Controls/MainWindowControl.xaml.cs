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
    /// Interaction logic for MainWindowControl.xaml
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        private readonly MainWindowControlViewModel _mainWindowControlViewModel;

        public MainWindowControl(MainWindowControlViewModel mainWindowControlViewModel)
        {
            _mainWindowControlViewModel = mainWindowControlViewModel;
            InitializeComponent();
            this.DataContext = mainWindowControlViewModel;
        }
    }
}
