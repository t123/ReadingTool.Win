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
using Ninject;
using RTWin.Entities;
using RTWin.Models;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for TermsControl.xaml
    /// </summary>
    public partial class TermsControl : UserControl
    {
        private readonly TermsControlViewModel _termsControlViewModel;
        public IList<TermModel> Terms { get; set; }

        public TermsControl(TermsControlViewModel termsControlViewModel)
        {
            _termsControlViewModel = termsControlViewModel;
            InitializeComponent();
            DataContext = _termsControlViewModel;
        }
    }
}
