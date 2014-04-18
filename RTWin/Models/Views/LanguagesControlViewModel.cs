using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using AutoMapper;
using RTWin.Annotations;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class LanguagesControlViewModel : INotifyPropertyChanged
    {
        private readonly LanguageService _languageService;
        private readonly LanguageCodeService _languageCodeService;
        private ObservableCollection<LanguageModel> _languages;
        private LanguageModel _selectedItem;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
            set { _saveCommand = value; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
            set { _cancelCommand = value; }
        }

        public ICommand AddCommand
        {
            get { return _addCommand; }
            set { _addCommand = value; }
        }

        public ICommand DeleteCommand
        {
            get { return _deleteCommand; }
            set { _deleteCommand = value; }
        }

        public ObservableCollection<LanguageModel> Languages
        {
            get { return _languages; }
            set { _languages = value; OnPropertyChanged("Languages"); }
        }

        private IEnumerable<LanguageCode> _codes;
        public IEnumerable<LanguageCode> Codes
        {
            get { return _codes; }
            set { _codes = value; }
        }

        public LanguageModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    Language = Mapper.Map<Language, LanguageModel>(_languageService.FindOne(_selectedItem.LanguageId));
                }

                OnPropertyChanged("SelectedItem");
            }
        }

        private LanguageModel _language;

        public LanguageModel Language
        {
            get { return _language; }
            set { _language = value; OnPropertyChanged("Language"); }
        }


        public LanguagesControlViewModel(LanguageService languageService, LanguageCodeService languageCodeService)
        {
            _languageService = languageService;
            _languageCodeService = languageCodeService;

            Codes = _languageCodeService.FindAll();
            MapCollection();
            SelectedItem = Languages.FirstOrDefault();

            _addCommand = new RelayCommand(param =>
            {
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
                            SentenceRegex = Language.SentenceRegex,
                            TermRegex = Language.TermRegex
                        }
                    };

                    _languageService.Save(language);
                    MapCollection();
                    SelectedItem = Languages.FirstOrDefault(x => x.LanguageId == language.LanguageId);
                }
            });

            _deleteCommand = new RelayCommand(param =>
            {
                var result = MessageBox.Show("Are you sure you want to delete this language?", "Delete Language", MessageBoxButton.OKCancel);

                if (result != MessageBoxResult.OK)
                    return;

                _languageService.DeleteOne(SelectedItem.LanguageId);
                MapCollection();
                SelectedItem = Languages.FirstOrDefault();
            }, param => SelectedItem != null);

            _saveCommand = new RelayCommand(param =>
            {
                var language = _languageService.FindOne(Language.LanguageId);
                var plugins = (from PluginLanguage item in Language.Plugins where item != null && item.Enabled select item.PluginId).ToArray();

                language.IsArchived = Language.IsArchived;
                language.LanguageCode = Language.LanguageCode;
                language.Name = Language.Name;
                language.Settings.Direction = Language.Direction;
                language.Settings.SentenceRegex = Language.SentenceRegex;
                language.Settings.TermRegex = Language.TermRegex;

                _languageService.Save(language, plugins);
                MapCollection();
                SelectedItem = Languages.FirstOrDefault(x => x.LanguageId == language.LanguageId);
            }, param => SelectedItem != null);

            _cancelCommand = new RelayCommand(param =>
            {
                Language = Mapper.Map<Language, LanguageModel>(_languageService.FindOne(SelectedItem.LanguageId));
                SelectedItem = Languages.FirstOrDefault(x => x.LanguageId == Language.LanguageId);
            }, param => SelectedItem != null);
        }

        private void MapCollection()
        {
            var languages = Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageModel>>(_languageService.FindAll());
            Languages = new ObservableCollection<LanguageModel>(languages);
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
