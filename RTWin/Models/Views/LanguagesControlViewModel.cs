using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using RTWin.Entities;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class LanguagesControlViewModel : BaseViewModel
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
            get
            {
                if (_languages == null)
                {
                    _languages = new ObservableCollection<LanguageModel>(Mapper.Map<IEnumerable<Language>, IEnumerable<LanguageModel>>(_languageService.FindAll()));
                }

                return _languages;
            }
            set
            {
                _languages = value;
                OnPropertyChanged("Languages");
            }
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
                OnPropertyChanged("SelectedItem");
            }
        }

        public LanguagesControlViewModel(LanguageService languageService, LanguageCodeService languageCodeService)
        {
            _languageService = languageService;
            _languageCodeService = languageCodeService;

            Codes = _languageCodeService.FindAll();
            SelectedItem = Languages.FirstOrDefault();

            _addCommand = new RelayCommand(() =>
            {
                var language = Language.NewLanguage();
                _languageService.Save(language);
                var mapped = Mapper.Map<Language, LanguageModel>(language);
                Languages.Add(mapped);
                SelectedItem = mapped;
            });

            _deleteCommand = new RelayCommand(() =>
            {
                var result = MessageBox.Show(
                    string.Format("Are you sure you want to delete {0}?", SelectedItem.Name),
                    string.Format("Delete {0}", SelectedItem.Name),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation
                    );

                if (result == MessageBoxResult.Yes)
                {
                    _languageService.DeleteOne(SelectedItem.LanguageId);
                    Languages.Remove(SelectedItem);
                    SelectedItem = Languages.FirstOrDefault();
                }
            }, () => SelectedItem != null);

            _saveCommand = new RelayCommand(() =>
            {
                var mapped = SelectedItem.ToLanguage();
                _languageService.Save(mapped, SelectedItem.Plugins.Where(x => x.Enabled).Select(x => x.PluginId));
            }, () => SelectedItem != null && SelectedItem.IsValid);
        }
    }
}
