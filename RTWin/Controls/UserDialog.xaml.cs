﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for UserDialog.xaml
    /// </summary>
    public partial class UserDialog : Window
    {
        private readonly UserService _userService;

        public UserDialog(UserService userService)
        {
            _userService = userService;
            InitializeComponent();

            this.ShowInTaskbar = false;

            BindUsers();

            this.Loaded += (sender, args) =>
            {
                if (App.User != null)
                {
                    this.DialogResult = true;
                    this.Close();
                }
            };
        }

        private void BindUsers()
        {
            var users = _userService.FindAll();

            if (users.Count == 1)
            {
                App.User = users.First();
                return;
            }

            ListBoxUsers.ItemsSource = users;
            ListBoxUsers.DisplayMemberPath = "Username";
        }

        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var user = ListBoxUsers.SelectedItem as User;

            if (user == null)
            {
                MessageBox.Show("Please select a profile name and then click Open.", "Choose a profile", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            App.User = user;
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            PromptDialog promptDialog = new PromptDialog("Please enter a name for your profile", "");
            var result = promptDialog.ShowDialog();

            if (result == true)
            {
                string username = promptDialog.InputText.Trim();

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

                _userService.Save(new User()
                {
                    Username = username
                });

                BindUsers();
            }
        }

        private void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var user = ListBoxUsers.SelectedItem as User;

            if (user == null)
            {
                MessageBox.Show("Please select a profile name and then click Delete.", "Choose a profile", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this profile?", "Delete user profile", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _userService.DeleteOne(user.UserId);
            }
        }

        private void ButtonRename_OnClick(object sender, RoutedEventArgs e)
        {
            var user = ListBoxUsers.SelectedItem as User;

            if (user == null)
            {
                MessageBox.Show("Please select a profile name and then click Rename.", "Choose a profile", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PromptDialog promptDialog = new PromptDialog("Please enter a new name for your profile", "");
            var result = promptDialog.ShowDialog();

            if (result == true)
            {
                string username = promptDialog.InputText.Trim();

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

                user.Username = username;
                _userService.Save(user);

                BindUsers();
            }
        }

        private void ButtonQuit_OnClick(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}
