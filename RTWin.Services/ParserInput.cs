using System;
using System.Collections.Generic;
using System.Linq;
using RTWin.Entities;

namespace RTWin.Services
{
    public class ParserInput
    {
        public string Html { get; protected set; }
        public Item Item { get; protected set; }
        public Language Language1 { get; protected set; }
        public Language Language2 { get; protected set; }
        public Term[] Terms { get; protected set; }
        public bool AsParallel { get; protected set; }
        public Dictionary<string, Term> Lookup { get; protected set; }
        public string WebApiEndPoint { get; protected set; }
        public string SignalREndPoint { get; protected set; }

        public ParserInput()
        {
            AsParallel = false;
            Terms = new Term[0];
            Lookup = new Dictionary<string, Term>();
        }

        public ParserInput WithWebApiEndPoint(string endpoint)
        {
            WebApiEndPoint = endpoint;
            return this;
        }

        public ParserInput WithSignalREndPoint(string endpoint)
        {
            SignalREndPoint = endpoint;
            return this;
        }

        public ParserInput WithHtml(string html)
        {
            Html = html;
            return this;
        }

        public ParserInput IsParallel()
        {
            return IsParallel(true);
        }

        public ParserInput IsParallel(bool asParallel)
        {
            AsParallel = asParallel;
            return this;
        }

        public ParserInput WithTerms(IEnumerable<Term> terms)
        {
            if (terms != null)
            {
                Terms = terms.ToArray();
                Lookup = Terms.ToDictionary(x => x.Phrase.ToLowerInvariant(), x => x);
            }

            return this;
        }

        public ParserInput WithItem(Item item)
        {
            Item = item;
            return this;
        }

        public ParserInput WithLanguage1(Language language)
        {
            Language1 = language;
            return this;
        }

        public ParserInput WithLanguage2(Language language)
        {
            Language2 = language;
            return this;
        }
    }

    public class Srt
    {
        public int LineNo { get; set; }
        public string Content { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
    }

    public class ParserOutput
    {
        public string Xml { get; set; }
        public string Html { get; set; }
        public List<Srt> L1Srt { get; set; }
        public List<Srt> L2Srt { get; set; }
        public ParseStats Stats { get; set; }

        public class ParseStats
        {
            public int Known { get; set; }
            public int Unknown { get; set; }
            public int Ignored { get; set; }
            public int NotSeen { get; set; }
            public TimeSpan Time { get; set; }
            public int TotalTerms { get; set; }
            public int UniqueTerms { get; set; }
            public int UniqueKnown { get; set; }
            public int UniqueUnknown { get; set; }
            public int UniqueIgnored { get; set; }
            public int UniqueNotSeen { get; set; }
        }

        public ParserOutput()
        {
            Stats = new ParseStats();
        }
    }
}