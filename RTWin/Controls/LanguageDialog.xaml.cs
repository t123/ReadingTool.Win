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
using RTWin.Models;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for LanguageDialog.xaml
    /// </summary>
    public partial class LanguageDialog : UserControl
    {
        private LanguageCodeService _languageCodeService;
        private LanguageService _languageService;
        private Language _language = null;
        private PluginService _pluginService;

        public LanguageModel Model { get; set; }

        public LanguageDialog(Language language)
        {
            InitializeComponent();

            _languageCodeService = App.Container.Get<LanguageCodeService>();
            _languageService = App.Container.Get<LanguageService>();
            _pluginService = App.Container.Get<PluginService>();


            BindLanguage(language);
        }

        private void BindLanguage(Language language)
        {
            Model = new LanguageModel();
            _language = language;

            if (_language == null)
            {
                Model.SentenceRegex = "[^\\.!\\?]+[\\.!\\?]+";
                Model.TermRegex = "([a-zA-ZÀ-ÖØ-öø-ȳ\\'-]+)";
                Model.Direction = Direction.LeftToRight;
            }
            else
            {
                Model.LanguageId = _language.LanguageId;
                Model.LanguageCode = _language.LanguageCode;
                Model.IsArchived = _language.IsArchived;
                Model.Name = _language.Name;
                Model.SentenceRegex = _language.Settings.SentenceRegex;
                Model.TermRegex = _language.Settings.TermRegex;
                Model.Direction = _language.Settings.Direction;
            }

            Model.Codes = _languageCodeService.FindAll().Select(x => new LanguageCodeModel { Code = x.Code, Name = x.Name }).ToList();
            var items = _pluginService.FindAllWithActive(_language == null ? 0 : _language.LanguageId);

            foreach (var item in items)
            {
                Model.Plugins.Add(new PluginLanguage()
                {
                    PluginId = long.Parse(item[0].ToString()),
                    Name = item[1].ToString(),
                    Description = item[2].ToString(),
                    Enabled = item[3] != null
                });
            }

            this.DataContext = Model;
        }

        public void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            switch (button.Tag.ToString())
            {
                case "Save":
                    SaveLanguage();
                    break;

                case "Cancel":
                    var language = _languageService.FindOne(_language.LanguageId);
                    BindLanguage(language);
                    break;
            }
        }

        private void SaveLanguage()
        {
            _language.IsArchived = CheckBoxArchive.IsChecked ?? false;
            _language.Name = TextBoxName.Text;
            _language.LanguageCode = ComboBoxLanguageCode.SelectedValue.ToString();
            var settings = _language.Settings;

            settings.Direction = RbLTR.IsChecked ?? false ? Direction.LeftToRight : (RbRTL.IsChecked ?? false ? Direction.RightToLeft : Direction.LeftToRight);
            settings.SentenceRegex = TextBoxSentenceRegex.Text;
            settings.TermRegex = TextBoxTermRegex.Text;

            _language.Settings = settings;

            var plugins = (from PluginLanguage item in ListBoxPlugins.Items where item != null && item.Enabled select item.PluginId).ToArray();

            _languageService.Save(_language, plugins);

            BindLanguage(_language);
        }
    }
}
