using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTWin.Messages
{
    public class ReadMessage
    {
        public long ItemId { get; set; }
        public bool AsParallel { get; set; }
    }
}
