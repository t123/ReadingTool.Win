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
    public class VideoParserService : BaseParserService
    {
        private ParserInput _pi;
        private readonly ParserOutput _po;

        public VideoParserService()
        {
            _po = new ParserOutput();
            _xsltFile = "video.xstl";
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

        public override ParserOutput Parse(ParserInput pi)
        {
            _pi = pi;
            _po.L1Srt = ParseSubtitles(_pi.Item.L1Content);
            _po.L2Srt = _pi.AsParallel ? ParseSubtitles(_pi.Item.L2Content) : new List<Srt>();

            var l1SentenceRegex = new Regex(_pi.Language1.Settings.SentenceRegex);
            var l1TermRegex = new Regex(_pi.Language2.Settings.TermRegex);

            XDocument document = new XDocument();
            var rootNode = new XElement("root");

            var contentNode = new XElement("content");
            AddDataToContentNode(_pi, contentNode);

            var frequency = new Dictionary<string, int>();

            for (int i = 0; i < _po.L1Srt.Count; i++)
            {
                var l1Paragraph = _po.L1Srt[i];
                var l2Paragraph = _pi.AsParallel && _po.L2Srt != null && i < _po.L2Srt.Count ? _po.L2Srt[i] : null;

                var joinNode = new XElement("join");
                joinNode.SetAttributeValue("line", l1Paragraph.LineNo);

                var l1ParagraphNode = new XElement("paragraph");
                var l2ParagraphNode = new XElement("translation");

                l1ParagraphNode.SetAttributeValue("start", l1Paragraph.Start);
                l1ParagraphNode.SetAttributeValue("end", l1Paragraph.End);

                l2ParagraphNode.Value = l2Paragraph == null ? string.Empty : l2Paragraph.Content;
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
                        termNode.SetAttributeValue("phraseClass", termLower.Replace("'", "_").Replace("\"", "_"));

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

            _po.Xml = document.ToString();
            _po.Html = _pi.Html.Replace("<!-- table -->", ApplyTransform(document, _xsltFile));

            return _po;
        }
    }
}
