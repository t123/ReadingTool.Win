using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using RTWin.Entities;

namespace RTWin.Services
{
    public class VideoParserService : IParserService
    {
        public class Srt
        {
            public int LineNo { get; set; }
            public string Content { get; set; }
            public double Start { get; set; }
            public double End { get; set; }
        }

        private readonly ParserInput _pi;
        private readonly ParserOutput _po;

        public List<Srt> L1Srt { get; private set; }
        public List<Srt> L2Srt { get; private set; }

        public VideoParserService(ParserInput pi)
        {
            _pi = pi;
            _po = new ParserOutput();
        }

        private List<Srt> ParseSubtitles(string content)
        {
            var lines = (content.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
            var srtList = new List<Srt>();

            Srt srt = null;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int lineNo;

                if (int.TryParse(line, out lineNo))
                {
                    if (srt != null)
                    {
                        srtList.Add(srt);
                    }

                    srt = new Srt();

                    srt.LineNo = lineNo;
                }
                else if (line.Contains("-->"))
                {
                    string[] times = line.Split(new[] { " --> " }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    DateTime start = DateTime.ParseExact(times.First(), "hh:mm:ss,fff", CultureInfo.InvariantCulture);
                    DateTime end = DateTime.ParseExact(times.Last(), "hh:mm:ss,fff", CultureInfo.InvariantCulture);
                    srt.Start = start.Hour * 60 * 60 + start.Minute * 60 + start.Second + (double)start.Millisecond / 1000;
                    srt.End = end.Hour * 60 * 60 + end.Minute * 60 + end.Second + (double)end.Millisecond / 1000;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(srt.Content))
                    {
                        srt.Content = StripHtml(line);
                    }
                    else
                    {
                        srt.Content = srt.Content + " " + StripHtml(line);
                    }
                }
            }

            return srtList;
        }

        private string StripHtml(string content)
        {
            return Regex.Replace(content, @"<[^>]*>", String.Empty)
                .Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                ;
        }

        public ParserOutput Parse()
        {
            L1Srt = ParseSubtitles(_pi.Item.L1Content);
            L2Srt = _pi.AsParallel ? ParseSubtitles(_pi.Item.L2Content) : null;

            //string[] l1Paragraphs = SplitIntoParagraphs(_pi.Item.L1Content);
            //string[] l2Paragraphs = _pi.AsParallel ? SplitIntoParagraphs(_pi.Item.L2Content) : null;

            var l1SentenceRegex = new Regex(_pi.Language1.Settings.SentenceRegex);
            var l1TermRegex = new Regex(_pi.Language2.Settings.TermRegex);

            XDocument document = new XDocument();
            var rootNode = new XElement("root");

            var contentNode = new XElement("content");
            contentNode.SetAttributeValue("isParallel", _pi.AsParallel);
            contentNode.SetAttributeValue("collectionName", _pi.Item.CollectionName);
            contentNode.SetAttributeValue("collectionNo", _pi.Item.CollectionNo);
            contentNode.SetAttributeValue("dateCreated", _pi.Item.DateCreated);
            contentNode.SetAttributeValue("dateModified", _pi.Item.DateModified);
            contentNode.SetAttributeValue("lastRead", _pi.Item.LastRead);
            contentNode.SetAttributeValue("l1Title", _pi.Item.L1Title);
            contentNode.SetAttributeValue("l2Title", _pi.Item.L2Title);
            contentNode.SetAttributeValue("l1Id", _pi.Language1.LanguageId);
            contentNode.SetAttributeValue("itemId", _pi.Item.ItemId);
            contentNode.SetAttributeValue("itemType", _pi.Item.ItemType.ToString().ToLowerInvariant());
            contentNode.SetAttributeValue("l1Direction", _pi.Language1.Settings.Direction);
            contentNode.SetAttributeValue("l2Direction", _pi.AsParallel ? _pi.Language2.Settings.Direction.ToString() : "");

            if (!string.IsNullOrWhiteSpace(_pi.Item.MediaUri) && File.Exists(_pi.Item.MediaUri))
            {
                contentNode.SetAttributeValue("mediaUri", _pi.Item.MediaUri);
            }

            var frequency = new Dictionary<string, int>();

            for (int i = 0; i < L1Srt.Count; i++)
            {
                var l1Paragraph = L1Srt[i];
                var l2Paragraph = _pi.AsParallel && L2Srt != null && i < L2Srt.Count ? L2Srt[i] : null;

                var joinNode = new XElement("join");
                joinNode.SetAttributeValue("line", l1Paragraph.LineNo);

                var l1ParagraphNode = new XElement("paragraph");
                var l2ParagraphNode = new XElement("translation");

                l1ParagraphNode.SetAttributeValue("start", l1Paragraph.Start);
                l1ParagraphNode.SetAttributeValue("end", l1Paragraph.End);

                l2ParagraphNode.Value = l2Paragraph.Content;
                l1ParagraphNode.SetAttributeValue("direction", _pi.Language1.Settings.Direction == Direction.LeftToRight ? "ltr" : "rtl");
                l2ParagraphNode.SetAttributeValue("direction", _pi.AsParallel ? _pi.Language2.Settings.Direction == Direction.LeftToRight ? "ltr" : "rtl" : "ltr");

                var sentences = SplitIntoSentences(l1Paragraph.Content, l1SentenceRegex);

                foreach (var sentence in sentences)
                {
                    var sentenceNode = new XElement("sentence");

                    var terms = SplitIntoTerms(sentence, l1TermRegex);

                    foreach (var term in terms)
                    {
                        var termNode = new XElement("term");
                        var termLower = term.ToLowerInvariant();
                        termNode.Value = term;
                        termNode.SetAttributeValue("phrase", termLower);

                        if (l1TermRegex.IsMatch(termLower))
                        {
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
                            }
                            else
                            {
                                termNode.SetAttributeValue("state", TermState.NotSeen.ToString().ToLowerInvariant());
                            }
                        }
                        else
                        {
                            termNode.SetAttributeValue("isTerm", false);
                        }

                        sentenceNode.Add(termNode);
                    }

                    l1ParagraphNode.Add(sentenceNode);
                }


                joinNode.Add(l1ParagraphNode);
                joinNode.Add(l2ParagraphNode);

                contentNode.Add(joinNode);
            }

            rootNode.Add(contentNode);
            document.Add(rootNode);

            var totalTerms = frequency.Select(x => x.Value).Sum();
            var termsUpdate = document.Descendants("term").Where(x => x.Attribute("isTerm").Value == "true");
            foreach (var t in termsUpdate)
            {
                t.SetAttributeValue("occurrences", frequency[t.Value.ToLowerInvariant()]);
                t.SetAttributeValue("frequency", Math.Round((double)frequency[t.Value.ToLowerInvariant()] / (double)totalTerms * 100, 2));
            }

            WriteFile(_pi.Item.ItemId + ".xml", document.ToString());
            _po.Html = _pi.Html.Replace("<!-- table -->", ApplyTransform(document));
            WriteFile(_pi.Item.ItemId + ".html", _po.Html);

            return _po;
        }

        protected virtual void WriteFile(string filename, string content)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(path, filename), false, Encoding.UTF8))
            {
                sw.Write(content);
            }
        }

        protected virtual string ReadFile(string filename)
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", filename);

            using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        protected virtual string ApplyTransform(XDocument document)
        {
            string xslText = ReadFile("video.xslt");

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
