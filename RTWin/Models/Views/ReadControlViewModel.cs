using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.AspNet.SignalR.Client;
using RTWin.Entities;
using RTWin.Models.Dto;
using RTWin.Services;

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
            }
        }

        private HubConnection _hubConnection;
        private IHubProxy _mainHubProxy;

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

        public ICommand ChangeItem
        {
            get { return _changeItemCommand; }
            set { _changeItemCommand = value; }
        }

        public ICommand MarkKnownCommand
        {
            get { return _markKnownCommand; }
            set { _markKnownCommand = value; }
        }

        public ReadControlViewModel(ItemService itemService, DbSettingService settings)
        {
            _itemService = itemService;
            _settings = settings;
            //_hubConnection = new HubConnection(_databaseService.GetSetting<string>(DbSetting.Keys.BaseWebSignalRAddress));
            //_mainHubProxy = _hubConnection.CreateHubProxy("MainHub");
            //_hubConnection.Start();

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
                        Message = "Decrease read count";
                        break;

                    case "increaselisten":
                        Message = _itemService.ChangeStatistics(Item, "listen", 1);
                        break;

                    case "decreaselisten":
                        Message = _itemService.ChangeStatistics(Item, "listen", -1);
                        break;
                }

                //Task.Factory.StartNew(() =>
                //{
                //    //TODO fixme, evilness
                //    //send back message from html when complete and update then instead
                //    System.Threading.Thread.Sleep(4000);
                //    Messenger.Default.Send<RefreshItemsMessage>(new RefreshItemsMessage());
                //});
            });

            //_markKnownCommand = new RelayCommand(param => _mainHubProxy.Invoke("Send", new object[] { "markremainingasknown", "" }));

            _changeItemCommand = new RelayCommand<ItemModel>(param =>
            {

            }, param => true);
        }

        private void UpdateNextPrev()
        {
            var previous = _itemService.FindPrev(Item, 5);
            var next = _itemService.FindNext(Item, 5);

            ItemList = Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(next.Union(previous).OrderBy(x => x.CollectionNo)).ToList();
        }
    }
}
