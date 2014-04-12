using System;
using NPoco;

namespace RTWin.Entities
{
    [TableName("termlog")]
    [PrimaryKey("Id")]
    public class TermLog
    {
        public long Id { get; set; }
        public DateTime EntryDate { get; set; }
        public long TermId { get; set; }
        public TermState State { get; set; }
    }
}