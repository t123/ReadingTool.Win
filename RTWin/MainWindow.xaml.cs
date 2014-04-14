using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Policy;
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
        public class ItemModel
        {
            public long ItemId { get; set; }
            public long? CollectionNo { get; set; }
            public string CollectionName { get; set; }
            public string ItemType { get; set; }
            public bool HasMedia { get; set; }
            public bool IsParallel { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime DateModifed { get; set; }
            public DateTime? LastRead { get; set; }
            public string L1Title { get; set; }
            public string L2Title { get; set; }
            public string Language { get; set; }
            public int ReadTimes { get; set; }
            public int ListenedTimes { get; set; }
        }

        public class TermModel
        {
            public long TermId { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime DateModified { get; set; }
            public string Phrase { get; set; }
            public string Language { get; set; }
            public string State { get; set; }
            public string Sentence { get; set; }
            public string BasePhrase { get; set; }
            public string Definition { get; set; }
        }

        private ItemService _itemService;
        private LanguageService _languageService;
        private TermService _termService;
        private List<ItemModel> _items;
        private List<TermModel> _terms;

        public MainWindow()
        {
            InitializeComponent();

            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();
            _termService = App.Container.Get<TermService>();

            BindLanguages();
            BindItems();
            BindTerms();

            Title = string.Format("ReadingTool - {0}", App.User.Username);

            SetButtonVisibility();
        }

        private void SetButtonVisibility()
        {
            var item = DataGridItems.SelectedItem as ItemModel;

            if (item == null)
            {
                ButtonEdit.Visibility = ButtonDelete.Visibility = ButtonRead.Visibility = ButtonReadParallel.Visibility = Visibility.Hidden;
                return;
            }

            ButtonEdit.Visibility = ButtonDelete.Visibility = ButtonRead.Visibility = ButtonReadParallel.Visibility = Visibility.Visible;

            if (item.ItemType.Equals(ItemType.Text.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                ButtonRead.Content = "Read";
                ButtonReadParallel.Content = "Read Parallel";

                if (item.IsParallel)
                {
                    ButtonReadParallel.Visibility = Visibility.Visible;
                }
                else
                {
                    ButtonReadParallel.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                ButtonRead.Content = "Watch";
                ButtonReadParallel.Content = "Watch Parallel";

                if (item.IsParallel)
                {
                    ButtonReadParallel.Visibility = Visibility.Visible;
                }
                else
                {
                    ButtonReadParallel.Visibility = Visibility.Hidden;
                }
            }
        }

        private void BindLanguages()
        {
            ListBoxLanguageNames.ItemsSource = _languageService.FindAll();
            //ListBoxLanguageNames.DisplayMemberPath = "Name";

            ListBoxCollectionNames.ItemsSource = _itemService.FindAllCollectionNames(null);
        }

        private void BindItems()
        {
            var items = _itemService.FindAll();
            var languages = _languageService.FindAll().ToDictionary(x => x.LanguageId, x => x.Name);

            _items = new List<ItemModel>(items.Count);

            foreach (var item in items)
            {
                _items.Add(new ItemModel()
                {
                    CollectionName = item.CollectionName,
                    DateCreated = item.DateCreated,
                    CollectionNo = item.CollectionNo,
                    ItemType = item.ItemType.ToString(),
                    LastRead = item.LastRead,
                    DateModifed = item.DateModified,
                    HasMedia = !string.IsNullOrWhiteSpace(item.MediaUri),
                    IsParallel = !string.IsNullOrWhiteSpace(item.L2Content),
                    ItemId = item.ItemId,
                    L1Title = item.L1Title,
                    L2Title = item.L2Title,
                    Language = languages.ContainsKey(item.L1LanguageId) ? languages[item.L1LanguageId] : "Unknown",
                    ReadTimes = item.ReadTimes,
                    ListenedTimes = item.ListenedTimes
                });
            }

            DataGridItems.ItemsSource = _items;
        }

        private void BindTerms()
        {
            var terms = _termService.FindAll();
            var languages = _languageService.FindAll().ToDictionary(x => x.LanguageId, x => x.Name);

            _terms = new List<TermModel>();

            foreach (var term in terms)
            {
                _terms.Add(new TermModel()
                {
                    TermId = term.TermId,
                    DateCreated = term.DateCreated,
                    DateModified = term.DateModified,
                    Phrase = term.Phrase,
                    Sentence = term.Sentence,
                    BasePhrase = term.BasePhrase,
                    Definition = term.Definition,
                    State = term.State.ToString(),
                    Language = languages.ContainsKey(term.LanguageId) ? languages[term.LanguageId] : "Unknown"
                });
            }
            DataGridTerms.ItemsSource = _terms.OrderBy(x => x.Language).ThenBy(x => x.Phrase, StringComparer.InvariantCultureIgnoreCase);
        }

        private void ButtonNewLanguage_OnClick(object sender, RoutedEventArgs e)
        {
            var languagedialog = new LanguageDialog(null);

            var result = languagedialog.ShowDialog() ?? false;

            if (result)
            {
                BindLanguages();
            }
        }
        private void ButtonNewItem_OnClick(object sender, RoutedEventArgs e)
        {
            var itemDialog = new ItemDialog(null);
            var result = itemDialog.ShowDialog();

            if (result == true)
            {
                BindItems();
            }
        }

        private void ListBoxLanguageNames_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListBoxLanguageNames.SelectedIndex < 0)
            {
                return;
            }

            var language = ListBoxLanguageNames.SelectedValue as Language;

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

        private void ButtonChangeProfile_OnClick(object sender, RoutedEventArgs e)
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
            else
            {
                _itemService = App.Container.Get<ItemService>();
                _languageService = App.Container.Get<LanguageService>();
                _termService = App.Container.Get<TermService>();

                BindLanguages();
                BindItems();
                BindTerms();
            }
        }

        private void ButtonRead_OnClick(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedRowAsItem();
            ReadItem(item, false);
        }

        private void ButtonReadParallel_OnClick(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedRowAsItem();
            ReadItem(item, true);
        }

        private Item GetSelectedRowAsItem()
        {
            var obj = DataGridItems.SelectedItem as ItemModel;

            if (obj == null)
            {
                return null;
            }

            var item = _itemService.FindOne(obj.ItemId); //TODO fixme

            return item;
        }

        private void ReadItem(Item item, bool asParallel)
        {
            if (item == null)
            {
                return;
            }

            if (item.ItemType == ItemType.Text)
            {
                var rw = new ReadWindow(item, asParallel);
                rw.Owner = this;
                rw.ShowDialog();
            }
            else if (item.ItemType == ItemType.Video)
            {
                var ww = new WatchWindow(item, asParallel);
                ww.Owner = this;
                ww.ShowDialog();
            }
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedRowAsItem();

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

            SetButtonVisibility();
        }

        private void DataGridItems_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SetButtonVisibility();
        }

        private void ButtonPlugins_OnClick(object sender, RoutedEventArgs e)
        {
            PluginDialog pluginDialog = App.Container.Get<PluginDialog>();
            pluginDialog.ShowDialog();
        }

        private void DataGridItems_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = GetSelectedRowAsItem();
            ReadItem(item, !string.IsNullOrWhiteSpace(item.L2Content));
        }
    }
}
