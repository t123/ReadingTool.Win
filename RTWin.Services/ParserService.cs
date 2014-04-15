using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using RTWin.Entities;

namespace RTWin.Services
{
    public class ParserService : BaseParserService
    {
        private ParserInput _pi;
        private readonly ParserOutput _po;

        public ParserService()
        {
            _po = new ParserOutput();
            _xsltFile = "single.xslt";
        }

        public override ParserOutput Parse(ParserInput pi)
        {
            _pi = pi;

            string[] l1Paragraphs = SplitIntoParagraphs(_pi.Item.L1Content);
            string[] l2Paragraphs = _pi.AsParallel ? SplitIntoParagraphs(_pi.Item.L2Content) : null;

            var l1SentenceRegex = new Regex(_pi.Language1.Settings.SentenceRegex);
            var l1TermRegex = new Regex(_pi.Language2.Settings.TermRegex);

            XDocument document = new XDocument();
            var rootNode = new XElement("root");

            var contentNode = new XElement("content");
            AddDataToContentNode(pi, contentNode);

            var frequency = new Dictionary<string, int>();

            for (int i = 0; i < l1Paragraphs.Length; i++)
            {
                var l1Paragraph = l1Paragraphs[i];
                var l2Paragraph = _pi.AsParallel && l2Paragraphs != null && i < l2Paragraphs.Length ? l2Paragraphs[i] : string.Empty;

                var joinNode = new XElement("join");
                var l1ParagraphNode = new XElement("paragraph");
                var l2ParagraphNode = new XElement("translation");
                l2ParagraphNode.Value = l2Paragraph;
                l1ParagraphNode.SetAttributeValue("direction", _pi.Language1.Settings.Direction == Direction.LeftToRight ? "ltr" : "rtl");
                l2ParagraphNode.SetAttributeValue("direction", _pi.AsParallel ? _pi.Language2.Settings.Direction == Direction.LeftToRight ? "ltr" : "rtl" : "ltr");

                var sentences = SplitIntoSentences(l1Paragraph, l1SentenceRegex);

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
                                termNode.SetAttributeValue("definition", existing.FullDefinition);
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
