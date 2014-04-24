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
        public LanguagesControlViewModel LanguagesControlViewModel { get; set; }

        public LanguagesControl(LanguagesControlViewModel languagesControlViewModel)
        {
            LanguagesControlViewModel = languagesControlViewModel;
            InitializeComponent();

            this.DataContext = LanguagesControlViewModel;
        }
    }
}
