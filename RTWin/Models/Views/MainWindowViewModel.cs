using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
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

        private User _currentUser;
        public User CurrentUser
        {
            set
            {
                _currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
            get { return _currentUser; }
        }

        public MainWindowViewModel(MainWindowControl mainWindowControl)
        {
            _mainWindowControl = mainWindowControl;
            CurrentUser = App.User;
            CurrentView = mainWindowControl;
        }
    }
}