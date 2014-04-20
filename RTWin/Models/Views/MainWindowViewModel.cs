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
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.AspNet.SignalR.Messaging;
using Ninject;
using RTWin.Annotations;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Services;

namespace RTWin.Models.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public static MetroDialogSettings DialogSettings
        {
            get { return new MetroDialogSettings() {AnimateHide = false, AnimateShow = false}; }
        }

        private RelayCommand _changeViewCommand;

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

        private User _currentUser;
        private LanguagesControl _languagesControl;
        private TermsControl _termsControl;
        private TextsControl _textsControl;
        private PluginsControl _pluginsControl;
        private ReadControl _readControl;
        private MainWindowControl _mainWindowControl;
        private UserControl _currentView;

        public User CurrentUser
        {
            set
            {
                _currentUser = value;
                OnPropertyChanged("CurrentUser");
            }
            get { return _currentUser; }
        }

        public MainWindowViewModel(
            LanguagesControl languagesControl,
            TermsControl termsControl,
            TextsControl textsControl,
            PluginsControl pluginsControl,
            ReadControl readControl,
            MainWindowControl mainWindowControl
            )
        {
            _languagesControl = languagesControl;
            _termsControl = termsControl;
            _textsControl = textsControl;
            _pluginsControl = pluginsControl;
            _readControl = readControl;
            _mainWindowControl = mainWindowControl;

            CurrentUser = App.User;
            CurrentView = _languagesControl;
            _changeViewCommand = new RelayCommand(x => ChangeView(x.ToString()));

            Messenger.Default.Register<ReadMessage>(this, (action) =>
            {
                _readControl.View(action.ItemId, action.AsParallel);
                CurrentView = _readControl;
            });

            Messenger.Default.Register<ChangeViewMessage>(this, (action) => ChangeView(action.ViewName));
        }

        public Tuple<long, long> GetSub(double time)
        {
            return _readControl.GetSub(time);
        }

        private void ChangeView(string viewName)
        {
            viewName = viewName.ToLowerInvariant();

            switch (viewName)
            {
                case "main":
                    CurrentView = _mainWindowControl;
                    break;

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
                    CurrentView = _mainWindowControl;
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