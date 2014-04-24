namespace RTWin.Messages
{
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
}