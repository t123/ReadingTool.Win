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
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for TermsControl.xaml
    /// </summary>
    public partial class TermsControl : UserControl
    {
        private TermService _termService;
        public IList<TermModel> Terms { get; set; }

        public TermsControl()
        {
            InitializeComponent();
            SetButtonVisibility();
            BindTerms();
        }

        private void BindTerms()
        {
            _termService = App.Container.Get<TermService>();
            var languageService = App.Container.Get<LanguageService>();

            var terms = _termService.FindAll();
            var languages = languageService.FindAll().ToDictionary(x => x.LanguageId, x => x.Name);

            Terms = new List<TermModel>();

            foreach (var term in terms)
            {
                Terms.Add(new TermModel()
                {
                    TermId = term.TermId,
                    DateCreated = term.DateCreated,
                    DateModified = term.DateModified,
                    Phrase = term.Phrase,
                    Sentence = term.Sentence,
                    BasePhrase = term.BasePhrase,
                    Definition = term.Definition,
                    State = term.State.ToString(),
                    Language = languages.ContainsKey(term.LanguageId) ? languages[term.LanguageId] : "Unknown"
                });
            }

            this.DataContext = Terms.OrderBy(x => x.Language).ThenBy(x => x.Phrase, StringComparer.InvariantCultureIgnoreCase);
        }

        private void SetButtonVisibility()
        {
            if (DataGridTerms.SelectedIndex >= 0)
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

        private void DataGridTerms_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonVisibility();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
