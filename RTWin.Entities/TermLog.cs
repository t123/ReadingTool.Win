using System;
using NPoco;

namespace RTWin.Entities
{
    public enum TermType
    {
        Unknown = 0,
        Create = 1,
        Modify = 2,
        Delete = 3
    }

    [TableName("termlog")]
    [PrimaryKey("Id")]
    public class TermLog
    {
        public long Id { get; set; }
        public DateTime EntryDate { get; set; }
        public long TermId { get; set; }
        public TermState State { get; set; }
        public TermType Type { get; set; }
    }
}