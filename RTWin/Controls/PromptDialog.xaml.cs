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

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for PromptDialog.xaml
    /// </summary>
    public partial class PromptDialog : Window
    {
        public PromptDialog(string title, string prompt, string input = "")
        {
            InitializeComponent();

            Title = title;
            TextBlockPrompt.Text = prompt;
            TextBoxInput.Text = input;
            TextBoxInput.Focus();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Input))
            {
                return;
            }

            DialogResult = true;
            Close();
        }

        public string Input { get { return TextBoxInput.Text.Trim(); } }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
