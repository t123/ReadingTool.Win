using System;
using System.Collections.Generic;
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