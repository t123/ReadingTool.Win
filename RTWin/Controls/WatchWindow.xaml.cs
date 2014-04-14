using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Awesomium.Core;
using Awesomium.Core.Data;
using Microsoft.AspNet.SignalR.Client;
using Ninject;
using RTWin.Common;
using RTWin.Entities;
using RTWin.Services;
using MediaState = System.Windows.Controls.MediaState;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for WatchWindow.xaml
    /// </summary>
    public partial class WatchWindow
    {
        private readonly CommonWindow _cw;

        public WatchWindow(Item item, bool parallel)
        {
            InitializeComponent();

            _cw = new CommonWindow(item, parallel, new VideoParserService(), WebControl);
            this.Title = _cw.GetTitle();
        }

        public Tuple<long, long> GetSub(double time)
        {
            var l1Sub = _cw.Output.L1Srt.FirstOrDefault(x => x.Start < time && x.End > time);
            var l2Sub = _cw.Output.L2Srt.FirstOrDefault(x => x.Start < time && x.End > time);

            var l1 = l1Sub == null ? -1 : l1Sub.LineNo;
            var l2 = l2Sub == null ? -1 : l2Sub.LineNo;

            return new Tuple<long, long>(l1, l2);
        }
    }
}
