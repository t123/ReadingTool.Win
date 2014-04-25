using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTWin.Entities;

namespace RTWin.Messages
{
    public class RefreshItemsMessage
    {
        private List<Item> _items = new List<Item>();

        public List<Item> Items
        {
            get { return _items; }
            private set { _items = value; }
        }

        public RefreshItemsMessage()
            : this(new List<Item>())
        {
        }

        public RefreshItemsMessage(Item item)
        {
            Items.Add(item);
        }

        public RefreshItemsMessage(IEnumerable<Item> items)
        {
            if (items == null)
            {
                return;
            }

            Items = items.ToList();
        }
    }
}