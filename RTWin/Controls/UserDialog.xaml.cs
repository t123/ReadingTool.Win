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
using System.Windows.Shapes;
using RTWin.Entities;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for UserDialog.xaml
    /// </summary>
    public partial class UserDialog : Window
    {
        private UserDialogViewModel _userDialogViewModel;

        public UserDialogViewModel UserDialogViewModel
        {
            get { return _userDialogViewModel; }
            set { _userDialogViewModel = value; }
        }

        public UserDialog(UserDialogViewModel userDialogViewModel)
        {
            _userDialogViewModel = userDialogViewModel;
            InitializeComponent();
            this.DataContext = UserDialogViewModel;
        }
    }
}
