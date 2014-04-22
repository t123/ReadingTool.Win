using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
        private readonly DatabaseService _databaseService;
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

        public MainWindowControlViewModel(ItemService itemService, DatabaseService databaseService)
        {
            _itemService = itemService;
            _databaseService = databaseService;
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

            CheckLatestVersion();
        }

        private void CheckLatestVersion()
        {
            var apiServer = _databaseService.GetSetting<string>(DbSetting.Keys.ApiServer);
            var lastChecked = _databaseService.GetSetting<DateTime?>(DbSetting.Keys.LastVersionCheck);

            if (lastChecked.HasValue && (DateTime.Now - lastChecked.Value.ToLocalTime()).TotalHours < 24)
            {
                //return;
            }

            Uri uri = new Uri(apiServer, UriKind.Absolute);
            Uri checkUri = new Uri(uri, "v1/api/checklatestversion");

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += async (sender, args) =>
            {
                var ignoreVersion = _databaseService.GetSetting<string>(DbSetting.Keys.IgnoreUpdateVersion);

                if (args.Result == ignoreVersion)
                {
                    return;
                }

                //TODO check version
                _databaseService.SaveSetting(DbSetting.Keys.LastVersionCheck, DateTime.UtcNow.ToString("o"));

                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                var settings = MainWindowViewModel.DialogSettings;
                settings.FirstAuxiliaryButtonText = "Ignore";
                var result = await metroWindow.ShowMessageAsync("New version", "There is a new version available. Click OK to download.", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, settings);

                if (result == MessageDialogResult.Affirmative)
                {
                    //TODO launch browser etc
                }
                else if (result == MessageDialogResult.Negative)
                {

                }
                else if (result == MessageDialogResult.FirstAuxiliary)
                {
                    _databaseService.SaveSetting(DbSetting.Keys.IgnoreUpdateVersion, args.Result);
                }
            };

            wc.DownloadStringAsync(checkUri);
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
