using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTWin.Messages
{
    public class ReadMessage
    {
        public long ItemId { get; private set; }
        public bool AsParallel { get; private set; }

        public ReadMessage(long itemId)
            : this(itemId, true)
        {
        }

        public ReadMessage(long itemId, bool asParallel)
        {
            ItemId = itemId;
            AsParallel = asParallel;
        }
    }

    public class RefreshItemsMessage
    {
        public bool Refresh { get; private set; }

        public RefreshItemsMessage()
            : this(true)
        {
        }

        public RefreshItemsMessage(bool refresh)
        {
            Refresh = refresh;
        }
    }

    public class RefreshViewsMessage
    {
        public RefreshViewsMessage()
        {
        }
    }

    public class ChangeViewMessage
    {
        private readonly string _viewName;
        public const string Main = @"main";
        public const string Languages = @"languages";
        public const string Items = @"items";
        public const string Terms = @"terms";
        public const string Plugins = @"plugins";
        public const string Profiles = @"profiles";

        public ChangeViewMessage(string viewName)
        {
            _viewName = viewName;
        }

        public string ViewName
        {
            get { return _viewName; }
        }
    }
}
