using System.Windows;
using RTWin.Models.Views;

namespace RTWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel MainWindowViewModel { get; private set; }

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
            InitializeComponent();
            this.DataContext = MainWindowViewModel;
        }
    }
}