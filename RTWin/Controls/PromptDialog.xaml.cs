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
        public PromptDialog() : this("Prompt", "")
        {
        }

        public PromptDialog(string title, string input)
        {
            InitializeComponent();
            this.Title = title;
            TitleText = title;
            InputText = input;
        }

        public string TitleText
        {
            get { return TextBoxTitle.Text; }
            set { TextBoxTitle.Text = value; }
        }

        public string InputText
        {
            get { return TextBoxInput.Text; }
            set { TextBoxInput.Text = value; }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
