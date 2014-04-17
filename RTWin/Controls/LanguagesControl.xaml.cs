using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Ninject;
using RTWin.Entities;
using RTWin.Models;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for LanguagesControl.xaml
    /// </summary>
    public partial class LanguagesControl : UserControl
    {
        private LanguageService _languageService;
        public IList<Language> Languages { get; set; }

        public LanguagesControl()
        {
            InitializeComponent();
            BindLanguages();
        }

        private void BindLanguages()
        {
            _languageService = App.Container.Get<LanguageService>();
            Languages = _languageService.FindAll();
            this.DataContext = Languages;

            if (ListBoxLanguages.Items.Count > 0)
            {
                ListBoxLanguages.SelectedIndex = 0;
            }
            else
            {
                ContentControl.Content = "";
            }
        }


        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            switch (button.Tag.ToString())
            {
                case "Add":
                    PromptDialog promptDialog = new PromptDialog("Language Name", "Name ");
                    var result = promptDialog.ShowDialog();

                    if (result == true)
                    {
                        var language = new Language()
                        {
                            Name = promptDialog.Input,
                            IsArchived = false,
                            LanguageCode = "--",
                            Settings = new LanguageSettings()
                            {
                                Direction = Direction.LeftToRight,
                                SentenceRegex = "[^\\.!\\?]+[\\.!\\?]+",
                                TermRegex = "([a-zA-ZÀ-ÖØ-öø-ȳ\\'-]+)"
                            }
                        };

                        _languageService.Save(language);
                        BindLanguages();
                    }
                    return;

                case "Delete":
                    var item = ListBoxLanguages.SelectedItem as Language;

                    if (item == null)
                    {
                        return;
                    }

                    _languageService.DeleteOne(item.LanguageId);
                    BindLanguages();
                    break;
            }
        }

        private void ListBoxLanguages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var language = ListBoxLanguages.SelectedItem as Language;

            if (language == null)
            {
                return;
            }

            ContentControl.Content = new LanguageDialog(language);
        }
    }
}
