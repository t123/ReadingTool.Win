using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using RTWin.Core;
using RTWin.Core.Enums;
using RTWin.Entities;

namespace RTWin.Services
{
    public abstract class BaseParserService : IParserService
    {
        protected string _xsltFile;
        protected ParserInput _pi;
        protected ParserOutput _po;
        protected DateTime _startTime;
        protected DateTime _endTime;

        protected void StartTimer()
        {
            _startTime = DateTime.Now;
        }

        protected void EndTimer()
        {
            _endTime = DateTime.Now;

            if (_po == null)
            {
                return;
            }

            _po.Stats.Time = (_endTime - _startTime);
        }

        public abstract ParserOutput Parse(ParserInput pi);

        protected void AddDataToContentNode(XElement contentNode)
        {
            contentNode.SetAttributeValue("isParallel", _pi.AsParallel);
            contentNode.SetAttributeValue("signalR", _pi.SignalREndPoint);
            contentNode.SetAttributeValue("webApi", _pi.WebApiEndPoint);
            contentNode.SetAttributeValue("isParallel", _pi.AsParallel);
            contentNode.SetAttributeValue("collectionName", _pi.Item.CollectionName);
            contentNode.SetAttributeValue("collectionNo", _pi.Item.CollectionNo);
            contentNode.SetAttributeValue("dateCreated", _pi.Item.DateCreated);
            contentNode.SetAttributeValue("dateModified", _pi.Item.DateModified);
            contentNode.SetAttributeValue("lastRead", _pi.Item.LastRead);
            contentNode.SetAttributeValue("l1Title", _pi.Item.L1Title);
            contentNode.SetAttributeValue("l2Title", _pi.Item.L2Title);
            contentNode.SetAttributeValue("l1Id", _pi.Language1.LanguageId);
            contentNode.SetAttributeValue("itemType", _pi.Item.ItemType.ToString().ToLowerInvariant());
            contentNode.SetAttributeValue("itemId", _pi.Item.ItemId);
            contentNode.SetAttributeValue("l1Direction", _pi.Language1.Direction);
            contentNode.SetAttributeValue("l2Direction", _pi.AsParallel ? _pi.Language2.Direction.ToString() : "");
            contentNode.SetAttributeValue("l1Code", _pi.Language1.LanguageCode);
            contentNode.SetAttributeValue("l2Code", _pi.Language2 == null ? "" : _pi.Language2.LanguageCode);

            if (!string.IsNullOrWhiteSpace(_pi.Item.MediaUri) && File.Exists(_pi.Item.MediaUri))
            {
                contentNode.SetAttributeValue("mediaUri", _pi.Item.MediaUri);
            }
        }

        protected class NodeComparer : IEqualityComparer<XElement>
        {
            public bool Equals(XElement x, XElement y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Attribute("phrase").Value == y.Attribute("phrase").Value;
            }

            public int GetHashCode(XElement obj)
            {
                return obj.Attribute("phrase").Value.GetHashCode();
            }
        }

        protected void UniqueTerms(XDocument document)
        {
            var nodes = document.Descendants("term").Where(x => x.Attribute("isTerm").Value == "true").Distinct(new NodeComparer()).ToArray();

            _po.Stats.UniqueTerms = nodes.Count();
            _po.Stats.UniqueKnown = nodes.Count(x => x.Attribute("state").Value == TermState.Known.ToString().ToLowerInvariant());
            _po.Stats.UniqueUnknown = nodes.Count(x => x.Attribute("state").Value == TermState.Unknown.ToString().ToLowerInvariant());
            _po.Stats.UniqueIgnored = nodes.Count(x => x.Attribute("state").Value == TermState.Ignored.ToString().ToLowerInvariant());
            _po.Stats.UniqueNotSeen = nodes.Count(x => x.Attribute("state").Value == TermState.NotSeen.ToString().ToLowerInvariant());
        }

        protected void AddFrequencyDataToTermNodes(Dictionary<string, int> frequency, XDocument document)
        {
            var totalTerms = frequency.Select(x => x.Value).Sum();
            var termsUpdate = document.Descendants("term").Where(x => x.Attribute("isTerm").Value == "true");
            foreach (var t in termsUpdate)
            {
                t.SetAttributeValue("occurrences", frequency[t.Value.ToLowerInvariant()]);
                t.SetAttributeValue("frequency", Math.Round((double)frequency[t.Value.ToLowerInvariant()] / (double)totalTerms * 100, 2));
            }

            var common = document.Descendants("term")
                .Where(x => x.Attribute("isTerm").Value == "true" && x.Attribute("state").Value == TermState.NotSeen.ToString().ToLowerInvariant())
                .Distinct(new NodeComparer())
                .OrderByDescending(x => double.Parse(x.Attribute("frequency").Value))
                .Take(60);

            int counter = 0;
            foreach (var node in common)
            {
                foreach (var x in document.Descendants("term").Where(x => x.Attribute("phrase").Value == node.Attribute("phrase").Value))
                {
                    if (counter < 20)
                    {
                        x.SetAttributeValue("commonness", "high");
                    }
                    else if (counter < 40)
                    {
                        x.SetAttributeValue("commonness", "medium");
                    }
                    else
                    {
                        x.SetAttributeValue("commonness", "low");
                    }
                }

                counter++;
            }
        }

        protected XElement CreateTermNode(string term, Regex l1TermRegex, Dictionary<string, int> frequency)
        {
            var termNode = new XElement("term");
            var termLower = term.ToLowerInvariant();
            termNode.Value = term;
            termNode.SetAttributeValue("phrase", termLower);
            termNode.SetAttributeValue("phraseClass", termLower.Replace("'", "_").Replace("\"", "_"));

            if (l1TermRegex.IsMatch(termLower))
            {
                _po.Stats.TotalTerms++;
                termNode.SetAttributeValue("isTerm", true);

                if (frequency.ContainsKey(termLower))
                {
                    frequency[termLower] = frequency[termLower] + 1;
                }
                else
                {
                    frequency[termLower] = 1;
                }

                if (_pi.Lookup.ContainsKey(termLower))
                {
                    var existing = _pi.Lookup[termLower];
                    termNode.SetAttributeValue("state", existing.State.ToString().ToLowerInvariant());
                    termNode.SetAttributeValue("definition", existing.FullDefinition);

                    switch (existing.State)
                    {
                        case TermState.Known:
                            _po.Stats.Known++;
                            break;

                        case TermState.Unknown:
                            _po.Stats.Unknown++;
                            break;

                        case TermState.Ignored:
                            _po.Stats.Ignored++;
                            break;
                    }

                }
                else
                {
                    _po.Stats.Unknown++;
                    termNode.SetAttributeValue("state", TermState.NotSeen.ToString().ToLowerInvariant());
                }
            }
            else
            {
                termNode.SetAttributeValue("isTerm", false);

                if (string.IsNullOrWhiteSpace(termLower))
                {
                    termNode.SetAttributeValue("isWhitespace", true);
                }
            }

            return termNode;
        }

        protected string ApplyTransform(XDocument document)
        {
            string xslText = ContentLoader.Instance.Get(_xsltFile);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            //settings.Indent = true;

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(XmlReader.Create(new StringReader(xslText)));
                    xslt.Transform(document.CreateReader(), writer);
                }
            }

            return sb.ToString();
        }

        protected string[] SplitIntoTerms(string sentence, Regex regex)
        {
            var matches = regex.Split(sentence);
            return matches;
        }

        protected string[] SplitIntoSentences(string paragraph, Regex regex)
        {
            if (!paragraph.EndsWith("\n"))
            {
                paragraph += "\n";
            }

            var matches = regex.Matches(paragraph);

            if (matches.Count == 0)
            {
                return new[] { paragraph.Trim() };
            }

            return (from Match m in matches select m.Value).ToArray();
        }

        protected string[] SplitIntoParagraphs(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new string[0];

            return content.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
        }
    }
}