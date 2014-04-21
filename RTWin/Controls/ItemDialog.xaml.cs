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
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Ninject;
using Ninject.Parameters;
using RTWin.Entities;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ItemDialog.xaml
    /// </summary>
    public partial class ItemDialog : MetroWindow
    {
        private readonly ItemDialogViewModel _itemDialogViewModel;
        private Item _item;

        public ItemDialog(Item item)
        {
            var paramenter = new ConstructorArgument("item", item);
            _itemDialogViewModel = App.Container.Get<ItemDialogViewModel>(paramenter);
            InitializeComponent();
            this.DataContext = _itemDialogViewModel;
        }
    }
}
