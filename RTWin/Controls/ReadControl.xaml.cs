using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ninject;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Controls
{
    /// <summary>
    /// Interaction logic for ReadWindow.xaml
    /// </summary>
    public partial class ReadControl : UserControl
    {
        private CommonWindow _cw;
        private ItemService _itemService;
        public ReadControl()
        {
            _itemService = App.Container.Get<ItemService>();
            InitializeComponent();
            _cw = new CommonWindow(WebControl);
        }

        public void View(long itemId, bool parallel)
        {
            var item = _itemService.FindOne(itemId);
            View(item, parallel);
        }

        public void View(Item item, bool parallel)
        {
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
    }
}
