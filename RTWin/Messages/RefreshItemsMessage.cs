using RTWin.Entities;

namespace RTWin.Messages
{
    public class RefreshItemsMessage
    {
        public Item Item { get; private set; }

        public RefreshItemsMessage()
            : this(null)
        {
        }

        public RefreshItemsMessage(Item item)
        {
            Item = item;
        }
    }
}