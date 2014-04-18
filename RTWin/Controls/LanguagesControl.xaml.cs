using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Ninject;
using RTWin.Entities;
using RTWin.Models;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for LanguagesControl.xaml
    /// </summary>
    public partial class LanguagesControl : UserControl
    {
        private LanguagesControlViewModel _languagesControlViewModel;

        public LanguagesControlViewModel LanguagesControlViewModel
        {
            get { return _languagesControlViewModel; }
            set { _languagesControlViewModel = value; }
        }

        public LanguagesControl(LanguagesControlViewModel languagesControlViewModel)
        {
            _languagesControlViewModel = languagesControlViewModel;
            InitializeComponent();

            this.DataContext = LanguagesControlViewModel;
        }
    }
}
