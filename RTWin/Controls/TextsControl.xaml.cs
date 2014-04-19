using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ninject;
using Ninject.Parameters;
using RTWin.Entities;
using RTWin.Models;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for TextsControl.xaml
    /// </summary>
    public partial class TextsControl : UserControl
    {
        private readonly TextsControlViewModel _textsControlViewModel;

        public TextsControl(TextsControlViewModel textsControlViewModel)
        {
            _textsControlViewModel = textsControlViewModel;
            InitializeComponent();
            this.DataContext = textsControlViewModel;
        }

        
        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            //    case "Read":
            //        //mainWindow.ContentControl.Content = mainWindow.ReadControl;
            //        //mainWindow.ReadControl.View(item.ItemId, false);
            //        break;

            //    case "Read Parallel":
            //        //mainWindow.ContentControl.Content = mainWindow.ReadControl;
            //        //mainWindow.ReadControl.View(item.ItemId, true);
            //        break;
            //}
        }
    }
}
