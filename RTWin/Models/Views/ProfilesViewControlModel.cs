using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Input;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using RTWin.Entities;
using RTWin.Models.Dto;
using RTWin.Services;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace RTWin.Models.Views
{
    public class ProfilesControlViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private ObservableCollection<UserModel> _users;
        private UserModel _selectedItem;
        private ICommand _addCommand;
        private ICommand _deleteCommand;
        private ICommand _saveCommand;
        private ICommand _cancelCommand;
        private ICommand _switchCommand;

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

        public UserModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public ObservableCollection<UserModel> Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new ObservableCollection<UserModel>(Mapper.Map<IEnumerable<User>, IEnumerable<UserModel>>(_userService.FindAll()));
                }

                return _users;
            }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }

        public ProfilesControlViewModel(UserService userService)
        {
            _userService = userService;
            SelectedItem = Users.FirstOrDefault(x => x.UserId == App.User.UserId);

            _addCommand = new RelayCommand(() =>
            {
                var user = User.NewUser();
                _userService.Save(user);
                var mapped = Mapper.Map<User, UserModel>(user);
                Users.Add(mapped);
                SelectedItem = mapped;
            }
                );

            _deleteCommand = new RelayCommand(() =>
            {
                var result = MessageBox.Show(
                    string.Format("Are you sure you want to delete {0}?", SelectedItem.Username),
                    string.Format("Delete {0}", SelectedItem.Username),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation
                    );

                if (result == MessageBoxResult.Yes)
                {
                    _userService.DeleteOne(SelectedItem.UserId);
                    Users.Remove(SelectedItem);
                    SelectedItem = Users.FirstOrDefault();
                }
            }, () => SelectedItem != null && Users.Count() > 1 && SelectedItem.UserId != App.User.UserId);

            _saveCommand = new RelayCommand(() =>
            {
                var mapped = Mapper.Map<UserModel, User>(SelectedItem);
                _userService.Save(mapped);
            }, () => SelectedItem.IsValid);

            _switchCommand = new RelayCommand<User>(param =>
            {
                var user = _userService.FindOne(SelectedItem.UserId);
                user.LastLogin = DateTime.Now.ToUniversalTime();
                _userService.Save(user);

                SelectedItem = Users.FirstOrDefault(x => x.UserId == user.UserId);
                var mapped = Mapper.Map<UserModel, User>(SelectedItem);
                App.User = mapped;
            }, param => SelectedItem != null);
        }
    }
}
