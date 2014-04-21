using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class TermsControlViewModel : INotifyPropertyChanged
    {
        private readonly TermService _termService;
        private readonly LanguageService _languageService;
        private ObservableCollection<Term> _terms;
        private ICommand _exportCommand;
        private string _filterText;
        private ObservableCollection<string> _collectionNames;
        private string _selectedCollectionName;

        public ICommand ExportCommand
        {
            get { return _exportCommand; }
            set { _exportCommand = value; }
        }

        public string FilterText
        {
            get { return _filterText; }
            set { _filterText = value; OnPropertyChanged("FilterText"); MapCollection(); }
        }

        public ObservableCollection<string> CollectionNames
        {
            get { return _collectionNames; }
            set { _collectionNames = value; OnPropertyChanged("CollectionNames"); }
        }

        public string SelectedCollectionName
        {
            get { return _selectedCollectionName; }
            set
            {
                _selectedCollectionName = value;
                OnPropertyChanged("SelectedCollectionName");
                FilterText = value.Replace(@""" - """, @""" """);
            }
        }

        public ObservableCollection<Term> Terms
        {
            get { return _terms; }
            set { _terms = value; OnPropertyChanged("Terms"); }
        }

        public TermsControlViewModel(TermService termService, LanguageService languageService)
        {
            _termService = termService;
            _languageService = languageService;
            MapCollection();

            var languages = _languageService.FindAll()
                .Where(x => !x.IsArchived) //TODO fixme
                .OrderBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => "\"" + x.Name + "\"");

            var complete = new List<string>(languages);

            foreach (var language in languages)
            {
                complete.Add(string.Format("{0} #{1}", language, TermState.Known.ToString().ToLowerInvariant()));
                complete.Add(string.Format("{0} #{1}", language, TermState.Unknown.ToString().ToLowerInvariant()));
                complete.Add(string.Format("{0} #{1}", language, TermState.Ignored.ToString().ToLowerInvariant()));
            }

            CollectionNames = new ObservableCollection<string>(complete);

            _exportCommand = new RelayCommand(parma =>
            {

            });
        }

        private void MapCollection()
        {
            Terms = new ObservableCollection<Term>(_termService.Search(maxResults: 1000, filter: FilterText));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
