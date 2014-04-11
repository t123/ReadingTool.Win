using System;

namespace RTWin.Entities
{
    public class TermLog
    {
        public long Id { get; set; }
        public DateTime EntryDate { get; set; }
        public Term Term { get; set; }
        public TermState State { get; set; }
    }
}