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
    /// Interaction logic for ProfilesControl.xaml
    /// </summary>
    public partial class ProfilesControl : UserControl
    {
        private readonly ProfilesControlViewModel _profilesControlViewModel;

        public ProfilesControl(ProfilesControlViewModel profilesControlViewModel)
        {
            _profilesControlViewModel = profilesControlViewModel;
            InitializeComponent();
            this.DataContext = profilesControlViewModel;
        }
    }
}
