using System;
using System.Collections.Generic;

namespace RTWin.Entities
{
    public class Library
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public IList<Item> Items { get; set; }
    }
}