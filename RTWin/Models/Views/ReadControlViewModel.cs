using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.AspNet.SignalR.Client;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Services;

namespace RTWin.Models.Views
{
    //TODO fix this mess :(
    public class ReadControlViewModel : INotifyPropertyChanged
    {
        private readonly ItemService _itemService;
        private readonly DatabaseService _databaseService;
        private ICommand _backCommand;
        private ICommand _changeCommand;
        private ICommand _prevItemCommand;
        private ICommand _nextItemCommand;
        private ICommand _markKnownCommand;

        private Item _item;
        private Item _prevItem, _nextItem;
        private string _message;

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

        private bool _prevItemVisible;
        private string _prevItemTitle;
        public bool PrevItemVisible
        {
            get { return _prevItemVisible; }
            set { _prevItemVisible = value; OnPropertyChanged("PrevItemVisible"); }
        }

        public string PrevItemTitle
        {
            get { return _prevItemTitle; }
            set { _prevItemTitle = value; OnPropertyChanged("PrevItemTitle"); }
        }

        private bool _nextItemVisible;
        private string _nextItemTitle;
        private HubConnection _hubConnection;
        private IHubProxy _mainHubProxy;

        public bool NextItemVisible
        {
            get { return _nextItemVisible; }
            set { _nextItemVisible = value; OnPropertyChanged("NextItemVisible"); }
        }

        public string NextItemTitle
        {
            get { return _nextItemTitle; }
            set { _nextItemTitle = value; OnPropertyChanged("NextItemTitle"); }
        }


        public Item PrevItem
        {
            get { return _prevItem; }
            set
            {
                _prevItem = value;
                OnPropertyChanged("PrevItem");
            }
        }

        public Item NextItem
        {
            get { return _nextItem; }
            set
            {
                _nextItem = value;
                OnPropertyChanged("NextItem");
            }
        }

        public ICommand BackCommand
        {
            get { return _backCommand; }
            set { _backCommand = value; }
        }

        public ICommand ChangeCommand
        {
            get { return _changeCommand; }
            set { _changeCommand = value; }
        }

        public ICommand PrevItemCommand
        {
            get { return _prevItemCommand; }
            set { _prevItemCommand = value; }
        }

        public ICommand NextItemCommand
        {
            get { return _nextItemCommand; }
            set { _nextItemCommand = value; }
        }

        public ICommand MarkKnownCommand
        {
            get { return _markKnownCommand; }
            set { _markKnownCommand = value; }
        }

        public ReadControlViewModel(ItemService itemService, DatabaseService databaseService)
        {
            _itemService = itemService;
            _databaseService = databaseService;
            _hubConnection = new HubConnection(_databaseService.GetSetting<string>(DbSetting.Keys.BaseWebSignalRAddress));
            _mainHubProxy = _hubConnection.CreateHubProxy("MainHub");
            _hubConnection.Start();

            _changeCommand = new RelayCommand(param =>
            {
                switch (param.ToString().ToLowerInvariant())
                {
                    case "increaseread":
                        _mainHubProxy.Invoke("Send", new object[] { "read", 1 });
                        Message = "Increase read count";
                        break;

                    case "decreaseread":
                        _mainHubProxy.Invoke("Send", new object[] { "read", -1 });
                        Message = "Decrease read count";
                        break;

                    case "increaselisten":
                        _mainHubProxy.Invoke("Send", new object[] { "listen", 1 });
                        Message = "Increase listen count";
                        break;

                    case "decreaselisten":
                        _mainHubProxy.Invoke("Send", new object[] { "listen", -1 });
                        Message = "Decrease listen count";
                        break;
                }

                Task.Factory.StartNew(() =>
                {
                    //TODO fixme, evilness
                    //send back message from html when complete and update then instead
                    System.Threading.Thread.Sleep(4000);
                    Messenger.Default.Send<RefreshItemsMessage>(new RefreshItemsMessage());
                });
            });

            _markKnownCommand = new RelayCommand(param => _mainHubProxy.Invoke("Send", new object[] { "markremainingasknown", "" }));

            _prevItemCommand = new RelayCommand(param =>
            {
                Messenger.Default.Send<ReadMessage>(new ReadMessage(PrevItem.ItemId, PrevItem.IsParallel));
            }, param => PrevItem != null);

            _nextItemCommand = new RelayCommand(param =>
            {
                Messenger.Default.Send<ReadMessage>(new ReadMessage(NextItem.ItemId, NextItem.IsParallel));
            }, param => NextItem != null);

            _backCommand = new RelayCommand(param => Messenger.Default.Send<ChangeViewMessage>(new ChangeViewMessage(ChangeViewMessage.Items)));
        }

        private void UpdateNextPrev()
        {
            var items = _itemService.FindNextPrev(Item);
            NextItem = items.Item1;
            PrevItem = items.Item2;

            PrevItemTitle = PrevItem == null ? "" : string.Format("{0}. {1} - {2}", PrevItem.CollectionNo, PrevItem.CollectionName, PrevItem.L1Title);
            PrevItemVisible = PrevItem != null;
            NextItemTitle = NextItem == null ? "" : string.Format("{0}. {1} - {2}", NextItem.CollectionNo, NextItem.CollectionName, NextItem.L1Title);
            NextItemVisible = NextItem != null;
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
