using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using Ninject.Parameters;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Entities;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class TextsControlViewModel : INotifyPropertyChanged
    {

        private IList<ItemSearch> _root;
        private readonly ItemService _itemService;
        private readonly LanguageService _languageService;
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
        private ICommand _backCommand;
        private ICommand _searchCommand;

        public ICommand SearchCommand
        {
            get { return _searchCommand; }
            set { _searchCommand = value; }
        }

        public ICommand BackCommand
        {
            get { return _backCommand; }
            set { _backCommand = value; }
        }

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

                if (_selectedItem != null)
                {
                    if (_selectedItem.ItemType == RTWin.Entities.ItemType.Text)
                    {
                        ItemType = "Read";
                    }
                    else
                    {
                        ItemType = "Watch";
                    }
                }

                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged("Items"); }
        }

        public IList<ItemSearch> Root
        {
            get { return _root; }
            set { _root = value; OnPropertyChanged("Root"); }
        }

        public TextsControlViewModel(ItemService itemService, LanguageService languageService)
        {
            _itemService = itemService;
            _languageService = languageService;
            MapCollection();
            SelectedItem = Items.FirstOrDefault();
            ConstructTree();

            _addCommand = new RelayCommand(param =>
            {
                IParameter parameter = new ConstructorArgument("item", (Item)null);
                var itemDialog = App.Container.Get<ItemDialog>(parameter);
                var result = itemDialog.ShowDialog();

                if (result == true)
                {
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
                MapCollection();
                SelectedItem = Items.FirstOrDefault();
            }, param => SelectedItem != null);

            _readCommand = new RelayCommand(param =>
            {
                bool parallel = param.ToString() == "Determine" ? SelectedItem.IsParallel : param.ToString() != "Single";
                Messenger.Default.Send<ReadMessage>(new ReadMessage(SelectedItem.ItemId, parallel));
            }, param => param != null && SelectedItem != null);

            _searchCommand = new RelayCommand(param =>
            {
                var node = param as ItemSearch;
                if (node == null)
                {
                    return;
                }

                FilterText = node.Value;
            });

            _backCommand = new RelayCommand(param => Messenger.Default.Send<ChangeViewMessage>(new ChangeViewMessage(ChangeViewMessage.Main)));

            Messenger.Default.Register<RefreshItemsMessage>(this, (action) => MapCollection());
        }

        private void ConstructTree()
        {
            var languages = _languageService.FindAll().OrderBy(x => x.IsArchived);
            var nodes = new List<ItemSearch>();
            nodes.Add(new ItemSearch() { Name = "Parallel Items", Value = "#parallel" });
            nodes.Add(new ItemSearch() { Name = "Items with media", Value = "#media" });
            nodes.Add(new ItemSearch() { Name = "Text items", Value = "#text" });
            nodes.Add(new ItemSearch() { Name = "Video items", Value = "#video" });

            foreach (var l in languages)
            {
                var node = new ItemSearch()
                {
                    Name = l.Name,
                    Value = string.Format(@"""{0}""", l.Name),
                    IsExpanded = !l.IsArchived
                };

                var collections = _itemService.FindAllCollectionNames(l.LanguageId);

                foreach (var c in collections)
                {
                    if (string.IsNullOrWhiteSpace(c))
                    {
                        continue;
                    }

                    node.Children.Add(new ItemSearch()
                    {
                        Name = c,
                        Value = string.Format(@"""{0}"" ""{1}""", l.Name, c)
                    });
                }

                nodes.Add(node);
            }

            Root = nodes;
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
