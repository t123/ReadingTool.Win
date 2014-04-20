using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTWin.Models.Entities
{
    public class ItemSearch
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public bool IsExpanded { get; set; }
        public bool IsBold { get; set; }

        public IList<ItemSearch> Children { get; set; }

        public ItemSearch()
        {
            Children = new List<ItemSearch>();
        }
    }
}
