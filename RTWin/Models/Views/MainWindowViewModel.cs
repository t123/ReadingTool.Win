using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ninject;
using RTWin.Annotations;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private UserService _userService;
        private RelayCommand _changeProfileCommand;
        private RelayCommand _manageProfileCommand;
        private RelayCommand _changeViewCommand;
        private UserControl _currentView;
        private ObservableCollection<User> _users;

        public RelayCommand ChangeProfileCommand
        {
            get { return _changeProfileCommand; }
            set { _changeProfileCommand = value; }
        }

        public RelayCommand ManageProfileCommand
        {
            get { return _manageProfileCommand; }
            set { _manageProfileCommand = value; }
        }

        public RelayCommand ChangeViewCommand
        {
            get { return _changeViewCommand; }
            set { _changeViewCommand = value; }
        }

        public UserControl CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }

        private User _currentUser;
        private LanguagesControl _languagesControl;
        private TermsControl _termsControl;
        private TextsControl _textsControl;
        private PluginsControl _pluginsControl;

        public User CurrentUser
        {
            set
            {
                _currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
            get { return _currentUser; }
        }

        public MainWindowViewModel(UserService userService)
        {
            _userService = userService;
            _languagesControl = App.Container.Get<LanguagesControl>();
            _termsControl = App.Container.Get<TermsControl>();
            _textsControl = App.Container.Get<TextsControl>();
            _pluginsControl = App.Container.Get<PluginsControl>();

            Users = new ObservableCollection<User>(_userService.FindAll());
            CurrentUser = App.User;
            CurrentView = _languagesControl;

            _changeProfileCommand = new RelayCommand(x =>
            {
                var user = _userService.FindOne(long.Parse(x.ToString()));

                if (user == null)
                {
                    return;
                }

                CurrentUser = user;
                App.User = user;
            }, x => x != null);

            _manageProfileCommand = new RelayCommand(x =>
            {
                var userDialog = App.Container.Get<UserDialog>();
                var result = userDialog.ShowDialog();

                if (result == true)
                {
                    CurrentUser = App.User;
                }

                Users = new ObservableCollection<User>(_userService.FindAll());
            });

            _changeViewCommand = new RelayCommand(x => ChangeView(x.ToString()));
        }

        public void ChangeView(string viewName)
        {
            viewName = viewName.ToLowerInvariant();

            switch (viewName)
            {
                case "languages":
                    CurrentView = _languagesControl;
                    break;

                case "items":
                    CurrentView = _textsControl;
                    break;

                case "terms":
                    CurrentView = _termsControl;
                    break;

                case "plugins":
                    CurrentView = _pluginsControl;
                    break;

                default:
                    break;
            }
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