using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using RTWin.Annotations;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class ItemDialogViewModel : INotifyPropertyChanged
    {
        private readonly ItemService _itemService;
        private readonly LanguageService _languageService;
        private Item _item;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private ICommand _openCommand;
        private ObservableCollection<Language> _languages;
        private bool? _dialogResult;

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        public ICommand OpenCommand
        {
            get { return _openCommand; }
            set { _openCommand = value; }
        }

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

        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value ?? new Item()
                {
                    ItemType = ItemType.Text
                };

                OnPropertyChanged("Item");
            }
        }

        public bool Refresh { get; set; }

        public ObservableCollection<Language> Languages
        {
            get { return _languages; }
            set { _languages = value; OnPropertyChanged("Languages"); }
        }

        public ItemDialogViewModel(ItemService itemService, LanguageService languageService, Item item)
        {
            _itemService = itemService;
            _languageService = languageService;
            Item = item;

            Languages = new ObservableCollection<Language>(_languageService.FindAll());

            _openCommand = new RelayCommand(param =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                var result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    Item.MediaUri = openFileDialog.FileName;
                }
            });

            _saveCommand = new RelayCommand(param =>
            {
                Item i = _itemService.FindOne(Item.ItemId) ?? new Item();
                i.CollectionName = Item.CollectionName;
                i.CollectionNo = Item.CollectionNo;
                i.ItemId = Item.ItemId;
                i.ItemType = Item.ItemType;
                i.L1Content = Item.L1Content;
                i.L1LanguageId = Item.L1LanguageId;
                i.L1Title = Item.L1Title;
                i.L2Content = Item.L2Content;
                i.L2LanguageId = Item.L2LanguageId;
                i.L2Title = Item.L2Title;
                i.MediaUri = Item.MediaUri;

                _itemService.Save(item);

                Refresh = true;
            });

            _cancelCommand = new RelayCommand(param =>
            {
                Item = _itemService.FindOne(Item.ItemId);
            });
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
