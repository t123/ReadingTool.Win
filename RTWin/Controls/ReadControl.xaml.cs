using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Ninject;
using RTWin.Entities;
using RTWin.Messages;
using RTWin.Models.Views;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ReadWindow.xaml
    /// </summary>
    public partial class ReadControl : Window
    {
        private readonly ReadControlViewModel _readControlViewModel;
        private CommonWindow _cw;
        private ItemService _itemService;
        private CurrentState _state;
        private static readonly object _lock = new object();

        private class CurrentState
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
            public bool IsMaximized { get; set; }

            public CurrentState(double width, double height, double left, double top, bool isMaximized)
            {
                Width = width;
                Height = height;
                Left = left;
                Top = top;
                IsMaximized = isMaximized;
            }
        }

        public ReadControl(ReadControlViewModel readControlViewModel)
        {
            _readControlViewModel = readControlViewModel;
            _itemService = App.Container.Get<ItemService>();
            InitializeComponent();
            _cw = new CommonWindow(WebControl);
            _readControlViewModel.CW = _cw;

            this.DataContext = readControlViewModel;
        }

        public void View(long itemId, bool parallel)
        {
            var item = _itemService.FindOne(itemId);
            View(item, parallel);
        }

        public void View(Item item, bool parallel)
        {
            _readControlViewModel.Item = item;
            _cw.Read(item, parallel);
        }

        public Tuple<long, long> GetSub(double time)
        {
            var l1Sub = _cw.Output.L1Srt == null ? null : _cw.Output.L1Srt.FirstOrDefault(x => x.Start < time && x.End > time);
            var l2Sub = _cw.Output.L2Srt == null ? null : _cw.Output.L2Srt.FirstOrDefault(x => x.Start < time && x.End > time);

            var l1 = l1Sub == null ? -1 : l1Sub.LineNo;
            var l2 = l2Sub == null ? -1 : l2Sub.LineNo;

            return new Tuple<long, long>(l1, l2);
        }

        private void ReadControl_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (this.WindowStyle == WindowStyle.SingleBorderWindow)
                {
                    _state = new CurrentState(this.Width, this.Height, this.Left, this.Top, this.WindowState == WindowState.Maximized);

                    this.WindowStyle = WindowStyle.None;

                    if (_state.IsMaximized)
                    {
                        this.WindowState = WindowState.Normal;
                    }

                    this.WindowState = WindowState.Maximized;
                    this.Topmost = true;
                    FullscreenMessage.Opacity = 1.0;
                    FullscreenMessage.Visibility = Visibility.Visible;

                    var timer = new System.Timers.Timer();
                    timer.Interval = 3000;
                    timer.Elapsed += (o, args) =>
                    {
                        lock (_lock)
                        {
                            timer.Stop();
                            timer.Enabled = false;
                            HideCanvas();
                        }
                    };

                    timer.Enabled = true;
                    timer.Start();
                }
                else
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.Topmost = false;
                    this.Left = _state.Left;
                    this.Top = _state.Top;
                    this.Width = _state.Width;
                    this.Height = _state.Height;
                    this.WindowState = _state.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                }
            }
        }

        private void HideCanvas()
        {
            if (FullscreenMessage.Visibility == Visibility.Visible)
            {
                var timer = new System.Timers.Timer();
                timer.Interval = 100;
                timer.Elapsed += (o, args) =>
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (FullscreenMessage.Opacity <= 0.1)
                        {
                            timer.Stop();
                            timer.Enabled = false;
                            FullscreenMessage.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            FullscreenMessage.Opacity -= 0.1;
                        }
                    });
                };

                timer.Enabled = true;
                timer.Start();
            }
        }

        private void Canvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lock (_lock)
            {
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() => FullscreenMessage.Visibility = Visibility.Hidden);
            }
        }
    }
}
