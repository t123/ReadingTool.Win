using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Awesomium.Core;
using Ninject;
using RTWin.Common;
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
        private readonly ItemService _itemService;
        private readonly LanguageService _languageService;

        public MainWindow()
        {
            InitializeComponent();

            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();

            BindLanguages();
            BindItems();
        }

        private void BindLanguages()
        {
            lbLanguages.ItemsSource = _languageService.FindAll();
            lbLanguages.DisplayMemberPath = "Name";
        }

        private void BindItems()
        {
            lbItems.ItemsSource = _itemService.FindAll();
        }

        private void btnNewLanguage_Click(object sender, RoutedEventArgs e)
        {
            var ld = new LanguageDialog(null);

            var result = ld.ShowDialog() ?? false;

            if (result == true)
            {
                BindLanguages();
            }
        }

        private void btnNewItem_Click(object sender, RoutedEventArgs e)
        {
            var itemDialog = new ItemDialog(null);
            var result = itemDialog.ShowDialog();

            if (result == true)
            {
                BindItems();
            }
        }

        private void BtnView_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lbItems.SelectedValue as Item;

            if (item == null)
            {
                return;
            }

            if (item.ItemType == ItemType.Text)
            {
                var rw = new ReadWindow(item);
                rw.Owner = this;
                rw.ShowDialog();
            }
            else if (item.ItemType == ItemType.Video)
            {
                var ww = new WatchWindow(item);
                ww.Owner = this;
                ww.ShowDialog();
            }
        }

        private void btnEditLanguage_Click(object sender, RoutedEventArgs e)
        {
            var language = lbLanguages.SelectedValue as Language;

            if (language == null)
            {
                return;
            }

            LanguageDialog languageDialog = new LanguageDialog(language);
            var result = languageDialog.ShowDialog();
            if (result == true)
            {
                BindLanguages();
            }
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lbItems.SelectedValue as Item;

            if (item == null)
            {
                return;
            }

            ItemDialog itemDialog = new ItemDialog(item);
            var result = itemDialog.ShowDialog();
            if (result == true)
            {
                BindItems();
            }
        }
    }

}
