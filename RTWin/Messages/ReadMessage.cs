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
}
