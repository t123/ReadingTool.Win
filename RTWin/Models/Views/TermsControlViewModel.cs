using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using RTWin.Annotations;
using RTWin.Core;
using RTWin.Core.Enums;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class TermsControlViewModel : BaseViewModel
    {
        private IList<Term> _allTerms;
        private readonly TermService _termService;
        private readonly LanguageService _languageService;
        private ObservableCollection<TermModel> _terms;
        private ICommand _backCommand;
        private ICommand _exportCommand;
        private string _filterText;
        private ObservableCollection<string> _collectionNames;
        private string _selectedCollectionName;

        public ICommand ExportCommand
        {
            get { return _exportCommand; }
            set { _exportCommand = value; }
        }

        public ICommand BackCommand
        {
            get { return _backCommand; }
            set { _backCommand = value; }
        }

        public string FilterText
        {
            get { return _filterText; }
            set { _filterText = value; OnPropertyChanged("FilterText"); Terms = null; }
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

        public ObservableCollection<TermModel> Terms
        {
            get
            {
                if (_terms == null)
                {
                    MapTerms();
                }
                return _terms;
            }
            set { _terms = value; OnPropertyChanged("Terms"); }
        }

        public TermsControlViewModel(TermService termService, LanguageService languageService)
        {
            _termService = termService;
            _languageService = languageService;
            _allTerms = _termService.FindAll();

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

            _exportCommand = new RelayCommand(() =>
            {

            });
        }

        private void MapTerms()
        {
            var temp = _allTerms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterText ?? ""))
            {
                var predicate = PredicateBuilder.True<Term>();

                var matches = Regex.Matches(FilterText.ToLowerInvariant(), @"[\#\w]+|""[\w\s]*""");

                var filter = PredicateBuilder.True<Term>();

                foreach (Match match in matches)
                {
                    if (match.Value.StartsWith("#"))
                    {
                        if (match.Value.Length > 1)
                        {
                            var remainder = match.Value.Substring(1, match.Length - 1).ToLowerInvariant();
                            switch (remainder)
                            {
                                case "known":
                                    temp = temp.Where(x => x.State == TermState.Known);
                                    break;

                                case "unknown":
                                case "notknown":
                                    temp = temp.Where(x => x.State == TermState.Unknown);
                                    break;

                                case "notseen":
                                    temp = temp.Where(x => x.State == TermState.NotSeen);
                                    break;

                                case "none":
                                    temp = temp.Where(x => x.State == TermState.None);
                                    break;

                                case "ignore":
                                case "ignored":
                                    temp = temp.Where(x => x.State == TermState.Ignored);
                                    break;

                                case "new":
                                    temp = temp.Where(x => x.DateCreated > DateTime.Now.AddDays(-7));
                                    break;

                                case "recent":
                                    temp = temp.Where(x => x.DateModified > DateTime.Now.AddDays(-7));
                                    break;
                            }
                        }
                    }
                    else
                    {
                        string value = match.Value.Trim();
                        bool exact = false;

                        if (value.StartsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 1);
                            exact = true;
                        }

                        if (value.EndsWith("\""))
                        {
                            value = value.Substring(0, value.Length - 1);
                            exact = true;
                        }

                        if (value.Length > 0)
                        {
                            if (exact)
                            {
                                filter = filter.And(x => x.LowerPhrase == value || x.BasePhrase.ToLowerInvariant() == value || x.Language.ToLowerInvariant() == value);
                            }
                            else
                            {
                                filter = filter.And(x => x.LowerPhrase.StartsWith(value) || x.BasePhrase.ToLowerInvariant().StartsWith(value) || x.Language.ToLowerInvariant().StartsWith(value));
                            }
                        }
                    }
                }

                predicate = predicate.And(filter);
                temp = temp.Where(predicate);
            }

            temp = temp.OrderBy(x => x.Language).ThenBy(x => x.LowerPhrase);
            _terms = new ObservableCollection<TermModel>(Mapper.Map<IEnumerable<Term>, IEnumerable<TermModel>>(temp));
        }
    }
}
