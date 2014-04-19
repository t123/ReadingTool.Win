using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using Ninject.Parameters;
using RTWin.Annotations;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class TextsControlViewModel : INotifyPropertyChanged
    {
        private readonly ItemService _itemService;
        private ObservableCollection<Item> _items;
        private string _filterText;
        private ObservableCollection<string> _collectionNames;
        private string _selectedCollectionName;
        private Item _selectedItem;
        private ICommand _addCommand;
        private ICommand _editCommand;
        private ICommand _copyCommand;
        private ICommand _deleteCommand;
        private ICommand _readCommand;

        public ICommand ReadCommand
        {
            get { return _readCommand; }
            set { _readCommand = value; }
        }

        public ICommand EditCommand
        {
            get { return _editCommand; }
            set { _editCommand = value; }
        }
        public ICommand CopyCommand
        {
            get { return _copyCommand; }
            set { _copyCommand = value; }
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

        private string _itemType;
        public string ItemType
        {
            get { return _itemType; }
            set { _itemType = value; OnPropertyChanged("ItemType"); }
        }

        public Item SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem.ItemType == Entities.ItemType.Text)
                {
                    ItemType = "Read";
                }
                else
                {
                    ItemType = "Watch";
                }

                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged("Items"); }
        }

        public TextsControlViewModel(ItemService itemService)
        {
            _itemService = itemService;
            CollectionNames = new ObservableCollection<string>(_itemService.FindCollectionsPerLanguage());
            MapCollection();
            SelectedItem = Items.FirstOrDefault();

            _addCommand = new RelayCommand(param =>
            {
                IParameter parameter = new ConstructorArgument("item", (Item)null);
                var itemDialog = App.Container.Get<ItemDialog>(parameter);
                var result = itemDialog.ShowDialog();

                if (result == true)
                {
                    CollectionNames = new ObservableCollection<string>(_itemService.FindCollectionsPerLanguage());
                    MapCollection();
                }
            });

            _editCommand = new RelayCommand(param =>
            {
                var item = _itemService.FindOne(SelectedItem.ItemId);
                IParameter parameter = new ConstructorArgument("item", item);
                var itemDialog = App.Container.Get<ItemDialog>(parameter);
                var result = itemDialog.ShowDialog();

                if (result == true)
                {
                    var id = SelectedItem.ItemId;
                    CollectionNames = new ObservableCollection<string>(_itemService.FindCollectionsPerLanguage());
                    MapCollection();
                    SelectedItem = Items.FirstOrDefault(x => x.ItemId == id);
                }
            }, param => SelectedItem != null);

            _copyCommand = new RelayCommand(param =>
            {
                var actualItem = _itemService.FindOne(SelectedItem.ItemId);
                var newItem = new Item()
                {
                    CollectionName = actualItem.CollectionName,
                    CollectionNo = actualItem.CollectionNo,
                    ItemType = actualItem.ItemType,
                    L1Content = actualItem.L1Content,
                    L1LanguageId = actualItem.L1LanguageId,
                    L1Title = actualItem.L1Title + " (copy)",
                    L2Content = actualItem.L2Content,
                    L2LanguageId = actualItem.L2LanguageId,
                    L2Title = actualItem.L2Title,
                    MediaUri = actualItem.MediaUri,
                    UserId = actualItem.UserId,
                };

                _itemService.Save(newItem);
                MapCollection();
                SelectedItem = Items.FirstOrDefault(x => x.ItemId == newItem.ItemId);
            }, param => SelectedItem != null);

            _deleteCommand = new RelayCommand(param =>
            {
                _itemService.DeleteOne(SelectedItem.ItemId);
                CollectionNames = new ObservableCollection<string>(_itemService.FindCollectionsPerLanguage());
                MapCollection();
                SelectedItem = Items.FirstOrDefault();
            }, param => SelectedItem != null);

            _readCommand = new RelayCommand(param =>
                Messenger.Default.Send<ReadMessage>(new ReadMessage(SelectedItem.ItemId, param.ToString() != "Single")),
                param => param != null && SelectedItem != null);

            Messenger.Default.Register<RefreshItemsMessage>(this, (action) => MapCollection());
        }

        private void MapCollection()
        {
            Items = new ObservableCollection<Item>(_itemService.Search(maxResults: 200, filter: FilterText));
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
