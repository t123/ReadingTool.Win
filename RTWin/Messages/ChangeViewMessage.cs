namespace RTWin.Messages
{
    public class ChangeViewMessage
    {
        private readonly string _viewName;
        private readonly long? _itemId;
        private readonly bool? _isParallel;

        public const string Main = @"main";
        public const string Languages = @"languages";
        public const string Items = @"items";
        public const string Terms = @"terms";
        public const string Plugins = @"plugins";
        public const string Profiles = @"profiles";

        public ChangeViewMessage(string viewName)
            : this(viewName, null, false)
        {
        }

        public ChangeViewMessage(string viewName, long? itemId, bool? isParallel)
        {
            _viewName = viewName;
            _itemId = itemId;
            _isParallel = isParallel;
        }

        public bool IsItem
        {
            get { return _itemId.HasValue; }
        }

        public long ItemId
        {
            get { return _itemId ?? 0; }
        }

        public bool IsParallel
        {
            get { return _isParallel ?? false; }
        }

        public string ViewName
        {
            get { return _viewName; }
        }
    }
}