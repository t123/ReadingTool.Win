using System;

namespace RTWin.Entities
{
    public class Statistics
    {
        public long Id { get; set; }
        public Language Language { get; set; }
        public DateTime EntryDate { get; set; }
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Known { get; set; }
        public int Unknown { get; set; }
        public int Ignored { get; set; }
        public int NotSeen { get; set; }
    }
}