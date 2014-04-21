using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RTWin.Annotations;
using RTWin.Common;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class UserDialogViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private ICommand _openCommand;

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

        public ICommand RenameCommand
        {
            get { return _renameCommand; }
            set { _renameCommand = value; }
        }

        public ICommand OpenCommand
        {
            get { return _openCommand; }
            set { _openCommand = value; }
        }

        private ICommand _renameCommand;
        private ICommand _deleteCommand;
        private ICommand _addCommand;

        private ObservableCollection<User> _users;

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                OnPropertyChanged("SelectedUser");
            }
        }

        private User _selectedUser;


        public ObservableCollection<User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }

        private bool? _dialogResult;

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; OnPropertyChanged("DialogResult"); }
        }

        public UserDialogViewModel(UserService userService)
        {
            _userService = userService;
            Users = new ObservableCollection<User>(_userService.FindAll());

            OpenCommand = new RelayCommand(param =>
            {
                App.User = SelectedUser;
                DialogResult = true;
            }, param => SelectedUser != null);

            RenameCommand = new RelayCommand(param => Rename(), param => SelectedUser != null);
            DeleteCommand = new RelayCommand(param => Delete(), param => SelectedUser != null && Users.Count>1);
            AddCommand = new RelayCommand(param => Add());
        }

        private void Rename()
        {
            var promptDialog = new PromptDialog("Rename a profile", "New profile name", SelectedUser.Username);
            var result = promptDialog.ShowDialog();

            if (result == true)
            {
                string username = promptDialog.Input;

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Please enter a profile name.", "Empty profile name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var found = _userService.FindOneByUsername(username);

                if (found != null)
                {
                    MessageBox.Show("This profile name is already in use, please choose a different username.", "Profile name already used", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var oldUser = SelectedUser;
                SelectedUser.Username = username;
                _userService.Save(SelectedUser);
                Users = new ObservableCollection<User>(_userService.FindAll());
                SelectedUser = Users.FirstOrDefault(x => x.UserId == oldUser.UserId);
            }
        }

        private void Delete()
        {
            var result = MessageBox.Show("Are you sure you want to delete this profile?", "Delete user profile", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _userService.DeleteOne(SelectedUser.UserId);
                Users = new ObservableCollection<User>(_userService.FindAll());
                SelectedUser = Users.FirstOrDefault();
            }
        }

        private void Add()
        {
            PromptDialog promptDialog = new PromptDialog("Create a profile", "Profile name");
            var result = promptDialog.ShowDialog();

            if (result == true)
            {
                string username = promptDialog.Input;

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Please enter a profile name.", "Empty profile name", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var found = _userService.FindOneByUsername(username);

                if (found != null)
                {
                    MessageBox.Show("This profile name is already in use, please choose a different username.", "Profile name already used", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var user = new User()
                {
                    Username = username
                }; 
                _userService.Save(user);

                Users = new ObservableCollection<User>(_userService.FindAll());
                SelectedUser = Users.FirstOrDefault(x=>x.UserId==user.UserId);
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
