using System;

namespace RTWin.Models
{
    public class ItemModel
    {
        public long ItemId { get; set; }
        public long? CollectionNo { get; set; }
        public string CollectionName { get; set; }
        public string ItemType { get; set; }
        public bool HasMedia { get; set; }
        public bool IsParallel { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModifed { get; set; }
        public DateTime? LastRead { get; set; }
        public string L1Title { get; set; }
        public string L2Title { get; set; }
        public string Language { get; set; }
        public int ReadTimes { get; set; }
        public int ListenedTimes { get; set; }
    }
}