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
        public IList<LanguageViewModel> Languages { get; set; }

        public LanguagesControl()
        {
            InitializeComponent();
            BindLanguages();
            SetButtonVisibility();
        }

        private void BindLanguages()
        {
            _languageService = App.Container.Get<LanguageService>();
            var languages = _languageService.FindAll();
            Languages = new List<LanguageViewModel>();

            foreach (var language in languages)
            {
                var stats = _languageService.FindStatistics(language.LanguageId);

                var lvm = new LanguageViewModel()
                {
                    LanguageId = language.LanguageId,
                    Name = language.Name,
                    TotalItems = stats.Item1,
                    TotalTerms = stats.Item2,
                    TotalKnown = stats.Item3,
                    TotalUnknown = stats.Item4
                };

                Languages.Add(lvm);
            }

            this.DataContext = Languages;
        }

        private void SetButtonVisibility()
        {
            if (DataGridLanguages.SelectedIndex >= 0)
            {
                ButtonEdit.Visibility = Visibility.Visible;
                ButtonDelete.Visibility = Visibility.Visible;
            }
            else
            {
                ButtonEdit.Visibility = Visibility.Hidden;
                ButtonDelete.Visibility = Visibility.Hidden;
            }
        }

        private void DataGridLanguages_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonVisibility();
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
                    {
                        LanguageDialog languageDialog = new LanguageDialog(null);
                        if (languageDialog.ShowDialog() ?? false)
                        {
                            BindLanguages();
                            SetButtonVisibility();
                        }
                    }
                    break;

                case "Edit":
                    {
                        var selected = DataGridLanguages.SelectedItem as LanguageViewModel;

                        if (selected == null)
                        {
                            return;
                        }

                        var language = _languageService.FindOne(selected.LanguageId);

                        if (language == null)
                        {
                            return;
                        }

                        LanguageDialog languageDialog = new LanguageDialog(language);
                        languageDialog.ShowDialog();
                    }
                    break;

                case "Delete":
                    break;
            }
        }

        private void DataGridLanguages_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = DataGridLanguages.SelectedItem as LanguageViewModel;

            if (selected == null)
            {
                return;
            }

            var language = _languageService.FindOne(selected.LanguageId);

            if (language == null)
            {
                return;
            }

            LanguageDialog languageDialog = new LanguageDialog(language);
            var result = languageDialog.ShowDialog();

            if (result == true)
            {
                BindLanguages();
                SetButtonVisibility();
            }
        }
    }
}
