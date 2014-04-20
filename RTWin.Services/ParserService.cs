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
        public ParserService()
        {
            _po = new ParserOutput();
            _xsltFile = "text.xslt";
        }

        public override ParserOutput Parse(ParserInput pi)
        {
            StartTimer();
            _pi = pi;

            string[] l1Paragraphs = SplitIntoParagraphs(_pi.Item.L1Content);
            string[] l2Paragraphs = _pi.AsParallel ? SplitIntoParagraphs(_pi.Item.L2Content) : null;

            var l1SentenceRegex = new Regex(_pi.Language1.Settings.SentenceRegex);
            var l1TermRegex = new Regex(_pi.Language1.Settings.TermRegex);

            XDocument document = new XDocument();
            var rootNode = new XElement("root");

            var contentNode = new XElement("content");
            AddDataToContentNode(contentNode);

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
                        sentenceNode.Add(CreateTermNode(term, l1TermRegex, frequency));
                    }

                    l1ParagraphNode.Add(sentenceNode);
                }

                joinNode.Add(l1ParagraphNode);
                joinNode.Add(l2ParagraphNode);

                contentNode.Add(joinNode);
            }

            rootNode.Add(contentNode);
            document.Add(rootNode);

            AddFrequencyDataToTermNodes(frequency, document);

            _po.Xml = document.ToString();
            _po.Html = _pi.Html.Replace("<!-- table -->", ApplyTransform(document, _xsltFile));

            UniqueTerms(document);
            EndTimer();
            return _po;
        }
    }
}
