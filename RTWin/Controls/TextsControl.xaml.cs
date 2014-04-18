using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ninject;
using RTWin.Entities;
using RTWin.Models;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for TextsControl.xaml
    /// </summary>
    public partial class TextsControl : UserControl
    {
        private ItemService _itemService;
        private LanguageService _languageService;
        public IList<ItemModel> Items { get; set; }

        public TextsControl()
        {
            _itemService = App.Container.Get<ItemService>();
            _languageService = App.Container.Get<LanguageService>();

            InitializeComponent();
            BindTexts();
            SetButtonVisibility();
        }

        private void BindTexts()
        {
            var items = _itemService.FindAll();
            var languages = _languageService.FindAll().ToDictionary(x => x.LanguageId, x => x.Name);

            Items = new List<ItemModel>(items.Count);

            foreach (var item in items)
            {
                Items.Add(new ItemModel()
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

            this.DataContext = Items.OrderBy(x => x.Language).ThenBy(x => x.CollectionName).ThenBy(x => x.CollectionNo).ThenBy(x => x.L1Title);
        }

        private void SetButtonVisibility()
        {
            var item = DataGridTexts.SelectedItem as ItemModel;

            if (item == null)
            {
                ButtonEdit.Visibility = Visibility.Hidden;
                ButtonDelete.Visibility = Visibility.Hidden;
                ButtonRead.Visibility = Visibility.Hidden;
                ButtonReadParallel.Visibility = Visibility.Hidden;
                return;
            }

            ButtonEdit.Visibility = Visibility.Visible;
            ButtonDelete.Visibility = Visibility.Visible;
            ButtonRead.Visibility = Visibility.Visible;

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

        private void DataGridTexts_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetButtonVisibility();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button == null)
            {
                return;
            }

            if (button.Tag.ToString() == "Add")
            {
                ItemDialog itemDialog = new ItemDialog(null);
                var result = itemDialog.ShowDialog();

                if (result == true)
                {
                    BindTexts();
                    SetButtonVisibility();
                }

                return;
            }

            var item = DataGridTexts.SelectedItem as ItemModel;

            if (item == null)
            {
                return;
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            switch (button.Tag.ToString())
            {
                case "Delete":
                    _itemService.DeleteOne(item.ItemId);
                    BindTexts();
                    SetButtonVisibility();
                    break;

                case "Edit":
                    {
                        ItemDialog itemDialog = new ItemDialog(item.ItemId);
                        var result = itemDialog.ShowDialog();

                        if (result == true)
                        {
                            BindTexts();
                            SetButtonVisibility();
                        }
                    }
                    break;

                case "Copy":
                    {
                        var actualItem = _itemService.FindOne(item.ItemId);
                        var newItem = new Item()
                        {
                            CollectionName = actualItem.CollectionName,
                            CollectionNo = actualItem.CollectionNo,
                            ItemType = actualItem.ItemType,
                            L1Content = actualItem.L1Content,
                            L1LanguageId = actualItem.L1LanguageId,
                            L1Title = actualItem.L1Title,
                            L2Content = actualItem.L2Content,
                            L2LanguageId = actualItem.L2LanguageId,
                            L2Title = actualItem.L2Title,
                            MediaUri = actualItem.MediaUri,
                            UserId = actualItem.UserId,
                        };

                        _itemService.Save(newItem);
                        BindTexts();
                        SetButtonVisibility();

                        break;
                    }

                case "Read":
                    //mainWindow.ContentControl.Content = mainWindow.ReadControl;
                    //mainWindow.ReadControl.View(item.ItemId, false);
                    break;

                case "Read Parallel":
                    //mainWindow.ContentControl.Content = mainWindow.ReadControl;
                    //mainWindow.ReadControl.View(item.ItemId, true);
                    break;
            }
        }

        private void DataGridTexts_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = DataGridTexts.SelectedItem as ItemModel;

            if (item == null)
            {
                return;
            }

            var mainWindow = Window.GetWindow(this) as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                //mainWindow.ContentControl.Content = mainWindow.ReadControl;
                //mainWindow.ReadControl.View(item.ItemId, item.IsParallel);
            }));
        }
    }
}
