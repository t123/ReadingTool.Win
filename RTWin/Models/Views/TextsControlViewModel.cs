using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using AutoMapper.Internal;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using Ninject.Parameters;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Core;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class TextsControlViewModel : BaseViewModel
    {
        private IList<Item> _allItems;
        private IEnumerable<Language> _languages;
        private IList<ItemSearch> _root;
        private readonly ItemService _itemService;
        private readonly LanguageService _languageService;
        private ObservableCollection<ItemModel> _items;
        private string _filterText;
        private ItemModel _selectedItem;
        private ICommand _addCommand;
        private ICommand _editCommand;
        private ICommand _copyCommand;
        private ICommand _deleteCommand;
        private ICommand _readCommand;
        private ICommand _searchCommand;

        public ICommand SearchCommand
        {
            get { return _searchCommand; }
            set { _searchCommand = value; }
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
            set
            {
                _filterText = value;
                OnPropertyChanged("FilterText");
                Items = null;
            }
        }

        private string _itemType;
        public string ItemType
        {
            get { return _itemType; }
            set { _itemType = value; OnPropertyChanged("ItemType"); }
        }

        public ItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    if (_selectedItem.ItemType == RTWin.Core.Enums.ItemType.Text)
                    {
                        ItemType = "Read";
                    }
                    else
                    {
                        ItemType = "Watch";
                    }
                }
                else
                {
                    ItemType = "Read";
                }

                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<ItemModel> Items
        {
            get
            {
                if (_items == null)
                {
                    MapItems();
                }

                return _items;
            }
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

            _languages = _languageService.FindAll();
            _allItems = _itemService.FindAll().ToList();

            SelectedItem = Items.FirstOrDefault();
            ConstructTree();

            _addCommand = new RelayCommand(() =>
            {
                var itemDialog = App.Container.Get<ItemDialog>();
                itemDialog.Show();
            });

            _editCommand = new RelayCommand<ItemModel>(param =>
            {
                var item = _itemService.FindOne(SelectedItem.ItemId);
                IParameter parameter = new ConstructorArgument("item", item);
                var itemDialog = App.Container.Get<ItemDialog>(parameter);
                itemDialog.Show();
            }, param => SelectedItem != null);

            _copyCommand = new RelayCommand<ItemModel>(param =>
            {
                var actualItem = _itemService.FindOne(SelectedItem.ItemId);
                var newItem = _itemService.CopyItem(actualItem);
                _itemService.Save(newItem);
                _allItems.Add(newItem);
                Items = null;
                SelectedItem = Items.FirstOrDefault(x => x.ItemId == newItem.ItemId);
            }, param => SelectedItem != null);

            _deleteCommand = new RelayCommand<ItemModel>(param =>
            {
                var result = MessageBox.Show(
                   string.Format("Are you sure you want to delete {0}?", SelectedItem.CommonName),
                   string.Format("Delete {0}", SelectedItem.CommonName),
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Exclamation
                   );

                if (result == MessageBoxResult.Yes)
                {
                    _itemService.DeleteOne(SelectedItem.ItemId);
                    _allItems.Remove(_allItems.First(x => x.ItemId == SelectedItem.ItemId));
                    Items = null;
                    ConstructTree();
                    SelectedItem = Items.FirstOrDefault();
                    Messenger.Default.Send<RefreshItemsMessage>(new RefreshItemsMessage());
                }
            }, param => SelectedItem != null);

            _readCommand = new RelayCommand<string>(param =>
            {
                bool parallel = param == null || param.ToString() == "Determine" ? SelectedItem.IsParallel : param.ToString() != "Single";
                Messenger.Default.Send<ReadMessage>(new ReadMessage(SelectedItem.ItemId, parallel));
            }, param => SelectedItem != null);

            _searchCommand = new RelayCommand<ItemSearch>(param =>
            {
                if (param == null)
                {
                    return;
                }

                FilterText = param.Value;
            });

            Messenger.Default.Register<RefreshItemsMessage>(this, param =>
            {
                if (param == null || param.Item == null)
                {
                    return;
                }

                if (_allItems.All(x => x.ItemId != param.Item.ItemId))
                {
                    _allItems.Add(param.Item);
                }
                else
                {
                    _allItems.Remove(_allItems.First(x => x.ItemId == param.Item.ItemId));
                    _allItems.Add(param.Item);
                }

                Items = null;
                ConstructTree();
            });
        }

        private void ConstructTree()
        {
            var filterText = FilterText;
            var nodes = new List<ItemSearch>();

            nodes.Add(new ItemSearch() { Name = "Parallel Items", Value = "#parallel" });
            nodes.Add(new ItemSearch() { Name = "Items with media", Value = "#media" });
            nodes.Add(new ItemSearch() { Name = "Text items", Value = "#text" });
            nodes.Add(new ItemSearch() { Name = "Video items", Value = "#video" });

            foreach (var l in _languages)
            {
                var node = new ItemSearch()
                {
                    Name = l.Name,
                    Value = string.Format(@"""{0}""", l.Name),
                    IsExpanded = !l.IsArchived
                };

                var collections = _allItems.Where(x => x.L1LanguageId == l.LanguageId && !string.IsNullOrWhiteSpace(x.CollectionName)).Select(x => x.CollectionName).Distinct();

                foreach (var c in collections)
                {
                    node.Children.Add(new ItemSearch()
                    {
                        Name = c,
                        Value = string.Format(@"""{0}"" ""{1}""", l.Name, c)
                    });
                }

                nodes.Add(node);
            }

            Root = nodes;
            FilterText = filterText;
        }

        private void MapItems()
        {
            var temp = _allItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FilterText ?? ""))
            {
                var predicate = PredicateBuilder.True<Item>();

                var matches = Regex.Matches(FilterText.ToLowerInvariant(), @"[\#\w]+|""[\w\s]*""");

                var filter = PredicateBuilder.True<Item>();

                foreach (Match match in matches)
                {
                    if (match.Value.StartsWith("#"))
                    {
                        if (match.Value.Length > 1)
                        {
                            var remainder = match.Value.Substring(1, match.Length - 1).ToLowerInvariant();
                            switch (remainder)
                            {
                                case "parallel":
                                    temp = temp.Where(x => x.IsParallel);
                                    break;

                                case "media":
                                    temp = temp.Where(x => x.HasMedia);
                                    break;

                                case "text":
                                    temp = temp.Where(x => x.ItemType == Core.Enums.ItemType.Text);
                                    break;

                                case "video":
                                    temp = temp.Where(x => x.ItemType == Core.Enums.ItemType.Video);
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
                                filter = filter.And(x => x.CollectionName.ToLowerInvariant() == value || x.L1Title.ToLowerInvariant() == value || x.L1Language.ToLowerInvariant() == value);
                            }
                            else
                            {
                                filter = filter.And(x => x.CollectionName.ToLowerInvariant().StartsWith(value) || x.L1Title.ToLowerInvariant().StartsWith(value) || x.L1Language.ToLowerInvariant().StartsWith(value));
                            }
                        }
                    }
                }

                predicate = predicate.And(filter);
                temp = temp.Where(predicate);
            }

            temp = temp.OrderBy(x => x.L1Language).ThenBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.L1Title);
            _items = new ObservableCollection<ItemModel>(Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(temp));
        }
    }
}
