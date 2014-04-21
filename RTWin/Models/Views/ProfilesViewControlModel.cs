using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class ProfilesControlViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private ObservableCollection<User> _users;
        private User _selectedItem;
        private User _user;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private ICommand _switchCommand;
        private ICommand _backCommand;

        public ICommand BackCommand
        {
            get { return _backCommand; }
            set { _backCommand = value; }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
            set { _saveCommand = value; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
            set { _cancelCommand = value; }
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

        public ICommand SwitchCommand
        {
            get { return _switchCommand; }
            set { _switchCommand = value; }
        }

        public User User
        {
            get { return _user; }
            set { _user = value; OnPropertyChanged("User"); }
        }

        public User SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    User = _userService.FindOne(_selectedItem.UserId);
                }

                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<User> Users
        {
            get { return _users; }
            set { _users = value; OnPropertyChanged("Users"); }
        }

        public ProfilesControlViewModel(UserService userService)
        {
            _userService = userService;
            _users = new ObservableCollection<User>(_userService.FindAll());
            SelectedItem = Users.FirstOrDefault(x => x.UserId == App.User.UserId);

            _addCommand = new RelayCommand(async param =>
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                var result = await metroWindow.ShowInputAsync("Please enter a name for your profile", "Profile Name", MainWindowViewModel.DialogSettings);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var user = new User()
                    {
                        Username = result.Trim(),
                    };

                    _userService.Save(user);
                    Users = new ObservableCollection<User>(_userService.FindAll());
                    SelectedItem = Users.FirstOrDefault(x => x.UserId == user.UserId);
                }
            });

            _deleteCommand = new RelayCommand(async param =>
            {
                var metroWindow = (Application.Current.MainWindow as MetroWindow);
                var result = await metroWindow.ShowMessageAsync("Delete " + SelectedItem.Username, "Are you sure you want to delete " + SelectedItem.Username + "?", MessageDialogStyle.AffirmativeAndNegative, MainWindowViewModel.DialogSettings);

                if (result == MessageDialogResult.Negative)
                    return;

                _userService.DeleteOne(SelectedItem.UserId);
                Users = new ObservableCollection<User>(_userService.FindAll());
                SelectedItem = Users.FirstOrDefault(x => x.UserId == App.User.UserId);
            }, param => SelectedItem != null && SelectedItem.UserId != App.User.UserId && Users.Count > 1);

            _saveCommand = new RelayCommand(param =>
            {
                var user = _userService.FindOne(SelectedItem.UserId);
                user.Username = User.Username;
                _userService.Save(user);
                Users = new ObservableCollection<User>(_userService.FindAll());
                SelectedItem = Users.FirstOrDefault(x => x.UserId == user.UserId);
            }, param => SelectedItem != null);

            _cancelCommand = new RelayCommand(param =>
            {
                User = _userService.FindOne(SelectedItem.UserId);
                SelectedItem = Users.FirstOrDefault(x => x.UserId == User.UserId);
            }, param => SelectedItem != null);

            _switchCommand = new RelayCommand(param =>
            {
                User = _userService.FindOne(SelectedItem.UserId);
                SelectedItem = Users.FirstOrDefault(x => x.UserId == User.UserId);
                App.User = SelectedItem;
            }, param => SelectedItem != null);

            _backCommand = new RelayCommand(param => Messenger.Default.Send<ChangeViewMessage>(new ChangeViewMessage(ChangeViewMessage.Main)));
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
