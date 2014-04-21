using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls.Dialogs;
using RTWin.Annotations;
using RTWin.Common;
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
            get { return new MetroDialogSettings() { AnimateHide = false, AnimateShow = false }; }
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
        private readonly ProfilesControl _profilesControl;
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
            MainWindowControl mainWindowControl,
            ProfilesControl profilesControl,
            UserService userService
            )
        {
            _languagesControl = languagesControl;
            _termsControl = termsControl;
            _textsControl = textsControl;
            _pluginsControl = pluginsControl;
            _readControl = readControl;
            _mainWindowControl = mainWindowControl;
            _profilesControl = profilesControl;

            var users = userService.FindAll();
            CurrentUser = App.User;

            if (users.Count() > 1)
            {
                ChangeView(ChangeViewMessage.Profiles);
            }
            else
            {
                ChangeView(ChangeViewMessage.Items);
            }

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
                case ChangeViewMessage.Main:
                    CurrentView = _mainWindowControl;
                    break;

                case ChangeViewMessage.Languages:
                    CurrentView = _languagesControl;
                    break;

                case ChangeViewMessage.Items:
                    CurrentView = _textsControl;
                    break;

                case ChangeViewMessage.Terms:
                    CurrentView = _termsControl;
                    break;

                case ChangeViewMessage.Plugins:
                    CurrentView = _pluginsControl;
                    break;

                case ChangeViewMessage.Profiles:
                    CurrentView = _profilesControl;
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