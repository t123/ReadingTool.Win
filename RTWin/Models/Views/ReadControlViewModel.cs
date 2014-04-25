using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.AspNet.SignalR.Client;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Dto;
using RTWin.Services;
using RTWin.Web;

namespace RTWin.Models.Views
{
    //TODO fix this mess :(
    public class ReadControlViewModel : BaseViewModel
    {
        private readonly ItemService _itemService;
        private readonly DbSettingService _settings;
        private ICommand _changeCommand;
        private ICommand _changeItemCommand;
        private ICommand _markKnownCommand;

        private Item _item;
        private string _message;
        private IList<ItemModel> _itemList;

        public string Message
        {
            get { return _message; }
            set { _message = value; OnPropertyChanged("Message"); }
        }

        public Item Item
        {
            get { return _item; }
            set
            {
                _item = value;
                OnPropertyChanged("Item");
                UpdateNextPrev();
                UpdateWindowTitle();
            }
        }

        private string _windowTitle;
        private ItemModel _nextToRead;

        public string WindowTitle
        {
            get { return _windowTitle; }
            set { _windowTitle = value; OnPropertyChanged("WindowTitle"); }
        }

        public ItemModel NextToRead
        {
            get { return _nextToRead; }
            set { _nextToRead = value; OnPropertyChanged("NextToRead"); }
        }

        public IList<ItemModel> ItemList
        {
            get { return _itemList; }
            set
            {
                _itemList = value;
                OnPropertyChanged("ItemList");
            }
        }

        public ICommand ChangeCommand
        {
            get { return _changeCommand; }
            set { _changeCommand = value; }
        }

        public ICommand ChangeItemCommand
        {
            get { return _changeItemCommand; }
            set { _changeItemCommand = value; }
        }

        public ICommand MarkKnownCommand
        {
            get { return _markKnownCommand; }
            set { _markKnownCommand = value; }
        }

        public CommonWindow CW { get; set; }
        private DateTime _startupTime;

        public ReadControlViewModel(ItemService itemService, DbSettingService settings)
        {
            _startupTime = DateTime.Now;
            _itemService = itemService;
            _settings = settings;

            var timer = new System.Timers.Timer();
            timer.Interval = 1000 * 60;
            timer.Elapsed += (sender, args) => System.Windows.Application.Current.Dispatcher.InvokeAsync(() => UpdateWindowTitle());
            timer.Start();

            _changeCommand = new RelayCommand<string>(param =>
            {
                if (string.IsNullOrWhiteSpace(param))
                {
                    return;

                }
                switch (param.ToLowerInvariant())
                {
                    case "increaseread":
                        Message = _itemService.ChangeStatistics(Item, "read", 1);
                        break;

                    case "decreaseread":
                        Message = _itemService.ChangeStatistics(Item, "read", -1);
                        break;

                    case "increaselisten":
                        Message = _itemService.ChangeStatistics(Item, "listen", 1);
                        break;

                    case "decreaselisten":
                        Message = _itemService.ChangeStatistics(Item, "listen", -1);
                        break;

                    default:
                        return;
                }

                Messenger.Default.Send(new RefreshItemsMessage());
            });

            _markKnownCommand = new RelayCommand(() => SignalRConnection.Instance.MainHubProxy.Invoke("Send", new object[] { "markremainingasknown", "" }));

            _changeItemCommand = new RelayCommand<ItemModel>(param =>
            {
                var item = _itemService.FindOne(param.ItemId);
                this.Item = item;
                CW.Read(item, param.IsParallel);
                Messenger.Default.Send(new RefreshItemsMessage());
            }, param => param != null);

            Messenger.Default.Send(new RefreshItemsMessage());
        }

        private void UpdateNextPrev()
        {
            var previous = _itemService.FindPrev(Item, 5);
            var next = _itemService.FindNext(Item, 5);

            var nextToRead = next.FirstOrDefault();

            if (nextToRead == null)
            {
                nextToRead = previous.FirstOrDefault();
            }

            NextToRead = Mapper.Map<Item, ItemModel>(nextToRead);
            ItemList = Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(next.Union(previous).OrderBy(x => x.CollectionNo)).ToList();
        }

        private void UpdateWindowTitle()
        {
            string name = string.IsNullOrEmpty(Item.CollectionName) ? "" : Item.CollectionName + " - ";
            name += Item.CollectionNo == null ? "" : Item.CollectionNo.ToString() + ". ";
            name += Item.L1Title;

            if (!string.IsNullOrWhiteSpace(Item.L1Language))
            {
                name += " (" + Item.L1Language + ")";
            }

            name += string.Format(" [this item {0:HH:mm}] [this window {1:HH:mm}] [{2} minutes]", DateTime.Now, _startupTime, Math.Floor((DateTime.Now - _startupTime).TotalMinutes));

            WindowTitle = name;
        }
    }
}
