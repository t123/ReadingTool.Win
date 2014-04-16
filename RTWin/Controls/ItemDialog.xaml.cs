using System;
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
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ItemDialog.xaml
    /// </summary>
    public partial class ItemDialog : Window
    {
        private ItemService _itemService;
        private LanguageService _languageService;
        private Item _item;

        public ItemDialog(long itemId)
        {
            _itemService = App.Container.Get<ItemService>();
            _item = _itemService.FindOne(itemId);
            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();

            InitializeComponent();
            BindLanguages();
            ResetFields();
            BindItem();
        }

        public ItemDialog(Item item)
        {
            _item = item;
            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();

            InitializeComponent();
            BindLanguages();
            ResetFields();
            BindItem();
        }

        private void BindItem()
        {
            if (_item == null)
            {
                return;
            }

            RbText.IsChecked = _item.ItemType == ItemType.Text;
            RbVideo.IsChecked = _item.ItemType == ItemType.Video;

            TextBoxCollectionName.Text = _item.CollectionName;
            TextBoxCollectionNo.Text = _item.CollectionNo.ToString();
            TextBoxMediaUri.Text = _item.MediaUri;

            TextBoxL1Title.Text = _item.L1Title;
            TextBoxL1Content.Text = _item.L1Content;
            ComboBoxL1Language.SelectedValue = _item.L1LanguageId;
            TextBoxL2Title.Text = _item.L2Title;
            TextBoxL2Content.Text = _item.L2Content;
            ComboBoxL2Language.SelectedValue = _item.L2LanguageId;
        }

        private void ResetFields()
        {
            RbText.IsChecked = true;
            RbVideo.IsChecked = false;

            TextBoxCollectionName.Text = "";
            TextBoxCollectionNo.Text = "";
            TextBoxMediaUri.Text = "";

            TextBoxL1Title.Text = "";
            TextBoxL1Content.Text = "";
            ComboBoxL1Language.SelectedIndex = 0;
            TextBoxL2Title.Text = "";
            TextBoxL2Content.Text = "";
            ComboBoxL2Language.SelectedIndex = 0;
        }

        private void BindLanguages()
        {
            var languages = _languageService.FindAll();
            ComboBoxL1Language.ItemsSource = languages;
            ComboBoxL2Language.ItemsSource = languages;
            ComboBoxL1Language.DisplayMemberPath = "Name";
            ComboBoxL2Language.DisplayMemberPath = "Name";
            ComboBoxL1Language.SelectedValuePath = "LanguageId";
            ComboBoxL2Language.SelectedValuePath = "LanguageId";
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (_item == null)
            {
                _item = new Item();
                _item.UserId = App.User.UserId;
            }

            _item.CollectionName = TextBoxCollectionName.Text;
            int collectionNo;
            if (int.TryParse(TextBoxCollectionNo.Text, out collectionNo))
            {
                _item.CollectionNo = collectionNo;
            }
            else
            {
                _item.CollectionNo = null;
            }

            _item.ItemType = (RbText.IsChecked ?? true) ? ItemType.Text : ItemType.Video;
            _item.MediaUri = TextBoxMediaUri.Text;
            _item.L1Content = TextBoxL1Content.Text;
            _item.L1Title = TextBoxL1Title.Text;
            _item.L1LanguageId = (long)ComboBoxL1Language.SelectedValue;
            _item.L2Content = TextBoxL2Content.Text;
            _item.L2Title = TextBoxL2Title.Text;
            _item.L2LanguageId = (long)ComboBoxL2Language.SelectedValue;

            _itemService.Save(_item);
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog(this);

            if (result == true)
            {
                TextBoxMediaUri.Text = openFileDialog.FileName;
            }
        }
    }
}
