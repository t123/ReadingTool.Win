using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MahApps.Metro.Controls;
using Ninject;
using RTWin.Controls;
using RTWin.Entities;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
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