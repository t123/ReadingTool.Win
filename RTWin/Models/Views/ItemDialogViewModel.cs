using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class ItemDialogViewModel : BaseViewModel
    {
        private readonly ItemService _itemService;
        private readonly LanguageService _languageService;
        private ItemModel _item;
        private ICommand _saveCommand;
        private ICommand _copyCommand;
        private ICommand _openCommand;
        private ICommand _splitCommand;
        private ObservableCollection<Language> _languages;
        public ICommand SplitCommand
        {
            get { return _splitCommand; }
            set { _splitCommand = value; }
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

        public ICommand CopyCommand
        {
            get { return _copyCommand; }
            set { _copyCommand = value; }
        }

        public ItemModel Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged("Item");
            }
        }

        public ObservableCollection<Language> Languages
        {
            get { return _languages; }
            set { _languages = value; OnPropertyChanged("Languages"); }
        }

        public ItemDialogViewModel(ItemService itemService, LanguageService languageService, Item item)
        {
            _itemService = itemService;
            _languageService = languageService;
            Languages = new ObservableCollection<Language>(_languageService.FindAll());
            MapItem(item);

            _openCommand = new RelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                var result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    Item.MediaUri = openFileDialog.FileName;
                }
            });

            _saveCommand = new RelayCommand(() =>
            {
                var newItem = Item.ToItem();
                _itemService.Save(newItem);
                MapItem(newItem);
                Messenger.Default.Send<RefreshItemsMessage>(new RefreshItemsMessage(newItem));
            });

            _copyCommand = new RelayCommand<ItemModel>(param =>
            {
                var newItem = _itemService.CopyItem(Item.ToItem());
                _itemService.Save(newItem);
                MapItem(newItem);
                Messenger.Default.Send<RefreshItemsMessage>(new RefreshItemsMessage(newItem));
            }, param => Item != null && Item.ItemId > 0);

            _splitCommand = new RelayCommand<ItemModel>(param =>
            {
                var items = _itemService.SplitItem(Item.ItemId);
                Messenger.Default.Send<RefreshItemsMessage>(new RefreshItemsMessage(items));
            }, param => Item != null && Item.ItemId > 0);
        }

        private void MapItem(Item item)
        {
            if (item == null)
            {
                item = Entities.Item.CreateItem();
            }

            Item = Mapper.Map<Item, ItemModel>(item);
        }
    }
}
