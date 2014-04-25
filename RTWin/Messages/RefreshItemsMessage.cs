using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTWin.Entities;

namespace RTWin.Messages
{
    public class RefreshItemsMessage
    {
        //public Item Item { get; private set; }
        public List<Item> Items { get; private set; }

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
                Items = new List<Item>();
                return;
            }

            Items = items.ToList();
        }
    }
}