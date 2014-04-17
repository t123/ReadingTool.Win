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
    [PrimaryKey("Id")]
    public class TermLog
    {
        public long Id { get; set; }
        public DateTime EntryDate { get; set; }
        public long TermId { get; set; }
        public TermState State { get; set; }
        public TermType Type { get; set; }
        public long LanguageId { get; set; }
    }

    public class TermStatistic
    {
        public class Types
        {
            public Dictionary<TermState, long> Created { get; set; }
            public Dictionary<TermState, long> Modified { get; set; }
            public Dictionary<TermState, long> Deleted { get; set; }

            public Types()
            {
                Created = new Dictionary<TermState, long>();
                Modified = new Dictionary<TermState, long>();
                Deleted = new Dictionary<TermState, long>();

                Created.Add(TermState.Ignored, 0);
                Created.Add(TermState.Known, 0);
                Created.Add(TermState.Unknown, 0);
                Created.Add(TermState.NotSeen, 0);
                Created.Add(TermState.None, 0);

                Modified.Add(TermState.Ignored, 0);
                Modified.Add(TermState.Known, 0);
                Modified.Add(TermState.Unknown, 0);
                Modified.Add(TermState.NotSeen, 0);
                Modified.Add(TermState.None, 0);

                Deleted.Add(TermState.Ignored, 0);
                Deleted.Add(TermState.Known, 0);
                Deleted.Add(TermState.Unknown, 0);
                Deleted.Add(TermState.NotSeen, 0);
                Deleted.Add(TermState.None, 0);
            }
        }

        public Dictionary<long, Types> PerLanguage { get; set; }

        public TermStatistic()
        {
            PerLanguage = new Dictionary<long, Types>();
        }
    }

    public class TermStatistics
    {
        public Dictionary<DateTime, TermStatistic> Statistics { get; set; }

        public TermStatistics()
        {
            Statistics = new Dictionary<DateTime, TermStatistic>();
        }
    }
}