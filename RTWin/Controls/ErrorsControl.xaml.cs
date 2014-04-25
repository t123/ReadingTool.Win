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
using NPoco.Expressions;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ErrorsControl.xaml
    /// </summary>
    public partial class ErrorsControl : UserControl
    {
        public ErrorsControl()
        {
            InitializeComponent();
            PopulateErrors();
        }

        private void PopulateErrors()
        {
            foreach (var error in Setup.Instance.Errors)
            {
                ErrorMessages.Inlines.Add(new Run(error.Value.Message) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });

                StringBuilder sb = new StringBuilder();
                var exception = error.Value.Exception;

                while (exception != null)
                {
                    sb.AppendLine("");
                    sb.AppendLine(exception.ToString());
                    exception = exception.InnerException;
                }

                sb.AppendLine("");
                sb.AppendLine("--------------------------------------------");
                sb.AppendLine("");

                ErrorMessages.Inlines.Add(new Run(sb.ToString()));
            }
        }
    }
}
