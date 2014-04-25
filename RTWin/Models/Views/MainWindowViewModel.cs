using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Dto;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly MainWindowControl _mainWindowControl;
        private readonly UserService _userService;
        private readonly ItemService _itemService;
        private readonly DbSettingService _settings;

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }

        public User CurrentUser
        {
            set
            {
                App.User = value;
                OnPropertyChanged("CurrentUser");
            }
            get
            {
                return App.User;
            }
        }

        private ICommand _readCommand;
        public ICommand ReadCommand
        {
            set { _readCommand = value; }
            get { return _readCommand; }
        }

        private ICommand _toolbarCommand;
        public ICommand ToolbarCommand
        {
            set { _toolbarCommand = value; }
            get { return _toolbarCommand; }
        }

        private ICommand _changeProfileCommand;
        public ICommand ChangeProfileCommand
        {
            set { _changeProfileCommand = value; }
            get { return _changeProfileCommand; }
        }

        private ObservableCollection<User> _users;

        public ObservableCollection<User> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new ObservableCollection<User>(_userService.FindAll());
                }
                return _users;
            }
            set { _users = value; OnPropertyChanged("Users"); }
        }

        private ObservableCollection<ItemModel> _newItems;

        public ObservableCollection<ItemModel> NewItems
        {
            get
            {
                if (_newItems == null)
                {
                    _newItems = new ObservableCollection<ItemModel>(Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(_itemService.FindRecentlyCreated(8)));
                }

                return _newItems;
            }
            set { _newItems = value; OnPropertyChanged("NewItems"); }
        }

        private ObservableCollection<ItemModel> _recentItems;

        public ObservableCollection<ItemModel> RecentItems
        {
            get
            {
                if (_recentItems == null)
                {
                    _recentItems = new ObservableCollection<ItemModel>(Mapper.Map<IEnumerable<Item>, IEnumerable<ItemModel>>(_itemService.FindRecentlyRead(8)));
                }

                return _recentItems;
            }
            set { _recentItems = value; OnPropertyChanged("RecentItems"); }
        }

        public MainWindowViewModel(MainWindowControl mainWindowControl, UserService userService, ItemService itemService, DbSettingService settings)
        {
            _mainWindowControl = mainWindowControl;
            _userService = userService;
            _itemService = itemService;
            _settings = settings;

            CurrentUser = userService.FindAll().OrderByDescending(x => x.LastLogin).First();

            CurrentView = mainWindowControl;

            _toolbarCommand = new RelayCommand<string>(PerformToolbarCommand);
            _changeProfileCommand = new RelayCommand<User>(PerformChangeProfile);
            _readCommand = new RelayCommand<ItemModel>(param => Read(param.ItemId, param.IsParallel));

            PerformToolbarCommand("items");

            Messenger.Default.Register<SwitchProfileMessage>(this, x => PerformChangeProfile(x.User));
            Messenger.Default.Register<ReadMessage>(this, x => Read(x.ItemId, x.AsParallel));
            Messenger.Default.Register<RefreshItemsMessage>(this, x => NewItems = null);

            CheckErrors();
            CheckLatestVersion();
        }

        private void CheckErrors()
        {
            if (Setup.Instance.HasErrors)
            {
                ErrorsControl errorsControl = new ErrorsControl();
                CurrentView = errorsControl;
            }
        }

        private void CheckLatestVersion()
        {
            var check = _settings.Get<bool?>(DbSetting.Keys.CheckNewVersions) ?? true;

            if (!check)
            {
                return;
            }

            var apiServer = _settings.Get<string>(DbSetting.Keys.ApiServer);
            var lastChecked = _settings.Get<DateTime?>(DbSetting.Keys.LastVersionCheck);

            if (lastChecked.HasValue && (DateTime.UtcNow - lastChecked.Value).TotalHours < 24)
            {
                return;
            }

            Uri uri = new Uri(apiServer, UriKind.Absolute);
            Uri checkUri = new Uri(uri, "v1/api/checklatestversion");

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += async (sender, args) =>
            {
                var ignoreVersion = _settings.Get<string>(DbSetting.Keys.IgnoreUpdateVersion);

                if (args.Result == ignoreVersion)
                {
                    return;
                }

                //TODO check version
                _settings.Save(DbSetting.Keys.LastVersionCheck, DateTime.UtcNow);

                var result = MessageBox.Show(
                    "There is a new version available. Click Ok to download, No to temporarily skip and Cancel to ignore this version.",
                    "New version",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Exclamation
                    );

                if (result == MessageBoxResult.OK)
                {
                    //TODO launch browser etc
                }
                else if (result == MessageBoxResult.No)
                {

                }
                else if (result == MessageBoxResult.Cancel)
                {
                    _settings.Save(DbSetting.Keys.IgnoreUpdateVersion, args.Result);
                }
            };

            wc.DownloadStringAsync(checkUri);
        }

        private void Read(long itemId, bool asParallel)
        {
            var readControl = App.Container.Get<ReadControl>();
            readControl.View(itemId, asParallel);
            readControl.Show();
            RecentItems = null;
        }

        private void PerformChangeProfile(User user)
        {
            if (user == null || user.UserId == CurrentUser.UserId)
            {
                return;
            }

            CurrentUser = user;
            PerformToolbarCommand("items");
        }

        private void PerformToolbarCommand(string command)
        {
            command = (command ?? "").Trim().ToLowerInvariant();

            switch (command)
            {
                case "additem":
                    var itemDialog = App.Container.Get<ItemDialog>();
                    itemDialog.Show();
                    return;

                case "profiles":
                    var profilesControl = App.Container.Get<ProfilesControl>();
                    CurrentView = profilesControl;
                    return;

                case "languages":
                    var languagesControl = App.Container.Get<LanguagesControl>();
                    CurrentView = languagesControl;
                    return;

                case "plugins":
                    var pluginsControl = App.Container.Get<PluginsControl>();
                    CurrentView = pluginsControl;
                    return;

                case "items":
                    var textsControl = App.Container.Get<TextsControl>();
                    CurrentView = textsControl;
                    return;

                case "terms":
                    var termsControl = App.Container.Get<TermsControl>();
                    CurrentView = termsControl;
                    return;

                case "settings":
                    var settingsControl = App.Container.Get<SettingsControl>();
                    CurrentView = settingsControl;
                    return;

                case "importtestdata":
                    Temp t = new Temp();
                    t.Languages();
                    t.Items();
                    t.Terms();
                    return;
            }
        }
    }
}