using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly MainWindowControl _mainWindowControl;
        private readonly UserService _userService;

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
            get { return _users; }
            set { _users = value; OnPropertyChanged("Users"); }
        }

        public MainWindowViewModel(MainWindowControl mainWindowControl, UserService userService)
        {
            _mainWindowControl = mainWindowControl;
            _userService = userService;

            CurrentUser = userService.FindAll().OrderByDescending(x => x.LastLogin).First();

            CurrentView = mainWindowControl;
            Users = new ObservableCollection<User>(_userService.FindAll());

            _toolbarCommand = new RelayCommand<string>(PerformToolbarCommand);
            _changeProfileCommand = new RelayCommand<User>(PerformChangeProfile);

            PerformToolbarCommand("items");

            Messenger.Default.Register<SwitchProfileMessage>(this, x => PerformChangeProfile(x.User));
        }

        private void PerformChangeProfile(User user)
        {
            if (user == null || user.UserId == CurrentUser.UserId)
            {
                return;
            }

            CurrentUser = user;
        }

        private void PerformToolbarCommand(string command)
        {
            command = (command ?? "").Trim().ToLowerInvariant();

            switch (command)
            {
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
            }
        }
    }
}