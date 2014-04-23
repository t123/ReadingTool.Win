using System;
using NPoco;
using RTWin.Core.Enums;

namespace RTWin.Entities
{
    [TableName("termlog")]
    public class TermLog
    {
        public DateTime EntryDate { get; set; }
        public long TermId { get; set; }
        public TermState State { get; set; }
        public TermType Type { get; set; }
        public long LanguageId { get; set; }
        public long UserId { get; set; }
    }
}