using System.Windows;
using Ninject;
using Ninject.Parameters;
using RTWin.Entities;
using RTWin.Models.Views;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ItemDialog.xaml
    /// </summary>
    public partial class ItemDialog : Window
    {
        private readonly ItemDialogViewModel _itemDialogViewModel;

        public ItemDialog()
            : this(null)
        {
        }

        public ItemDialog(Item item)
        {
            var paramenter = new ConstructorArgument("item", item);
            _itemDialogViewModel = App.Container.Get<ItemDialogViewModel>(paramenter);
            InitializeComponent();
            this.DataContext = _itemDialogViewModel;
        }
    }
}
