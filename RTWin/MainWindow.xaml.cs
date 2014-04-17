using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Ninject;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserService _userService;
        public ReadControl ReadControl { get; private set; }

        public LanguagesControl LanguagesControl { get; private set; }

        public TermsControl TermsControl { get; private set; }

        public TextsControl TextsControl { get; private set; }
        public PluginsControl PluginsControl { get; private set; }


        public IList<User> Users { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _userService = App.Container.Get<UserService>();

            SetContent();
        }

        private void SetContent()
        {
            HyperlinkTerms.FontWeight = FontWeights.Normal;
            HyperlinkItems.FontWeight = FontWeights.Normal;
            HyperlinkLanguages.FontWeight = FontWeights.Normal;
            HyperlinkPlugins.FontWeight = FontWeights.Normal;

            LanguagesControl = new LanguagesControl();
            TermsControl = new TermsControl();
            TextsControl = new TextsControl();
            ReadControl = new ReadControl();
            PluginsControl = new PluginsControl();

            Users = _userService.FindAll();
            this.DataContext = Users;

            Title = string.Format("ReadingTool - {0}", App.User.Username);

            if (TextsControl.Items.Count > 0)
            {
                ContentControl.Content = TextsControl;
                HyperlinkItems.FontWeight = FontWeights.Heavy;
            }
            else
            {
                ContentControl.Content = LanguagesControl;
                HyperlinkLanguages.FontWeight = FontWeights.Heavy;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = sender as Hyperlink;

            if (hl == null)
            {
                return;
            }

            HyperlinkTerms.FontWeight = FontWeights.Normal;
            HyperlinkItems.FontWeight = FontWeights.Normal;
            HyperlinkLanguages.FontWeight = FontWeights.Normal;
            HyperlinkPlugins.FontWeight = FontWeights.Normal;

            switch (hl.Tag.ToString())
            {
                case "Languages":
                    HyperlinkLanguages.FontWeight = FontWeights.Heavy;
                    ContentControl.Content = LanguagesControl;
                    break;

                case "Terms":
                    HyperlinkTerms.FontWeight = FontWeights.Heavy;
                    ContentControl.Content = TermsControl;
                    break;

                case "Items":
                    HyperlinkItems.FontWeight = FontWeights.Heavy;
                    ContentControl.Content = TextsControl;
                    break;

                case "Plugins":
                    HyperlinkPlugins.FontWeight = FontWeights.Heavy;
                    ContentControl.Content = PluginsControl;
                    break;

                default:
                    break;
            }
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            if (menuItem == null)
            {
                return;
            }

            switch (menuItem.Tag.ToString())
            {
                case "Maange Profiles":
                    ManageProfiles();
                    return;
            }
        }

        private void ManageProfiles()
        {
            var currentUser = App.User;
            App.User = null;
            var skip = new Ninject.Parameters.ConstructorArgument("skip", false);
            var userDialog = App.Container.Get<UserDialog>(skip);
            var result = userDialog.ShowDialog() ?? false;

            if (result == false)
            {
                App.User = currentUser;
            }

            SetContent();
        }

        private void Profile_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem == null)
            {
                return;
            }

            if (App.User.UserId.ToString() == menuItem.Tag.ToString())
            {
                return;
            }

            var currentUser = App.User;
            App.User = _userService.FindOne(int.Parse(menuItem.Tag.ToString()));

            if (App.User == null)
            {
                App.User = currentUser;
                return;
            }

            SetContent();
        }
    }
}