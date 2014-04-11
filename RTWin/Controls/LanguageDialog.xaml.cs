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
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for LanguageDialog.xaml
    /// </summary>
    public partial class LanguageDialog : Window
    {
        private LanguageCodeService _languageCodeService;
        private LanguageService _languageService;
        private Language _language = null;

        public LanguageDialog(Language language)
        {
            InitializeComponent();

            _language = language;
            _languageCodeService = App.Container.Get<LanguageCodeService>();
            _languageService = App.Container.Get<LanguageService>();

            BindLanguage();
            BindLanguages();
            ResetFields();
        }

        private void ResetFields()
        {
            TextBoxSentenceRegex.Text = "[^\\.!\\?]+[\\.!\\?]+";
            TextBoxTermRegex.Text = "([a-zA-ZÀ-ÖØ-öø-ȳ\\-\\']+)";
            RbLTR.IsChecked = true;
            ComboBoxLanguageCode.SelectedIndex = 0;
        }

        private void BindLanguages()
        {
            var languageCodes = _languageCodeService.FindAll();
            ComboBoxLanguageCode.ItemsSource = languageCodes;
            ComboBoxLanguageCode.DisplayMemberPath = "Name";
            ComboBoxLanguageCode.SelectedValuePath = "Code";
        }

        private void BindLanguage()
        {
            if (_language == null)
            {
                return;
            }

            CheckBoxArchive.IsChecked = _language.IsArchived;
            TextBoxName.Text = _language.Name;
            RbLTR.IsChecked = _language.Settings.Direction == Direction.LeftToRight;
            RbRTL.IsChecked = _language.Settings.Direction == Direction.RightToLeft;
            CheckBoxDisplaySpaces.IsChecked = _language.Settings.DisplaySpaces;
            CheckBoxPauseAudio.IsChecked = _language.Settings.PauseOnModal;
            TextBoxSentenceRegex.Text = _language.Settings.SentenceRegex;
            TextBoxTermRegex.Text = _language.Settings.TermRegex;
            ComboBoxLanguageCode.SelectedValue = _language.LanguageCode;
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (_language == null)
            {
                _language = new Language()
                {
                    IsArchived = CheckBoxArchive.IsChecked ?? false,
                    Name = TextBoxName.Text,
                    LanguageCode = ComboBoxLanguageCode.SelectedValue.ToString(),
                    UserId = App.User.UserId,
                    Settings = new LanguageSettings()
                    {
                        Direction = (RbLTR.IsChecked ?? true) ? Direction.LeftToRight : Direction.RightToLeft,
                        DisplaySpaces = CheckBoxArchive.IsChecked ?? true,
                        PauseOnModal = CheckBoxArchive.IsChecked ?? true,
                        SentenceRegex = TextBoxSentenceRegex.Text,
                        TermRegex = TextBoxTermRegex.Text
                    }
                };
            }
            else
            {
                _language.IsArchived = CheckBoxArchive.IsChecked ?? false;
                _language.Name = TextBoxName.Text;
                _language.LanguageCode = ComboBoxLanguageCode.SelectedValue.ToString();
                var settings = _language.Settings;

                settings.Direction = (RbLTR.IsChecked ?? true) ? Direction.LeftToRight : Direction.RightToLeft;
                settings.DisplaySpaces = CheckBoxArchive.IsChecked ?? true;
                settings.PauseOnModal = CheckBoxArchive.IsChecked ?? true;
                settings.SentenceRegex = TextBoxSentenceRegex.Text;
                settings.TermRegex = TextBoxTermRegex.Text;

                _language.Settings = settings;
            }

            _languageService.Save(_language);
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
