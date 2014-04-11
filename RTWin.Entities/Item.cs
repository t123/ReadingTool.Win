using System;
using NPoco;
using NPoco.FluentMappings;

namespace RTWin.Entities
{
    [TableName("item")]
    [PrimaryKey("ItemId")]
    public class Item
    {
        public long ItemId { get; set; }
        public ItemType ItemType { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime? LastRead { get; set; }
        public string MediaUri { get; set; }
        public string L1Title { get; set; }
        public string L1Content { get; set; }
        public long L1LanguageId { get; set; }
        public string L2Title { get; set; }
        public string L2Content { get; set; }
        public long L2LanguageId { get; set; }
        public long UserId { get; set; }
    }
}