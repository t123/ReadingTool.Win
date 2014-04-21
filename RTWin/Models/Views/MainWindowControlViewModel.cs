using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class MainWindowControlViewModel : INotifyPropertyChanged
    {
        private readonly ItemService _itemService;
        private ObservableCollection<Item> _items;
        private ICommand _readCommand;
        private RelayCommand _changeViewCommand;

        public RelayCommand ChangeViewCommand
        {
            get { return _changeViewCommand; }
            set { _changeViewCommand = value; }
        }

        public ICommand ReadCommand
        {
            get { return _readCommand; }
            set { _readCommand = value; }
        }
        public ObservableCollection<Item> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged("Items"); }
        }

        public MainWindowControlViewModel(ItemService itemService)
        {
            _itemService = itemService;
            Items = new ObservableCollection<Item>(_itemService.FindRecent(15));

            _changeViewCommand = new RelayCommand(param => Messenger.Default.Send<ChangeViewMessage>(new ChangeViewMessage(param.ToString())));

            _readCommand = new RelayCommand(param =>
            {
                var item = param as Item;

                if (item == null)
                {
                    return;
                }

                Messenger.Default.Send<ReadMessage>(new ReadMessage(item.ItemId, item.IsParallel));
                Items = new ObservableCollection<Item>(_itemService.FindRecent(15));
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
