using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
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

        public ParserInput()
        {
            AsParallel = false;
            Terms = new Term[0];
            Lookup = new Dictionary<string, Term>();
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

    public class ParserOutput
    {
        public string Html { get; set; }
    }

    public interface IParserService
    {
        ParserOutput Parse();
    }

    public class ParserService : IParserService
    {
        private readonly ParserInput _pi;
        private readonly ParserOutput _po;

        public ParserService(ParserInput pi)
        {
            _pi = pi;
            _po = new ParserOutput();
        }

        public ParserOutput Parse()
        {
            string[] l1Paragraphs = SplitIntoParagraphs(_pi.Item.L1Content);
            string[] l2Paragraphs = _pi.AsParallel ? SplitIntoParagraphs(_pi.Item.L2Content) : null;

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
            contentNode.SetAttributeValue("itemType", _pi.Item.ItemType.ToString().ToLowerInvariant());
            contentNode.SetAttributeValue("itemId", _pi.Item.ItemId);

            if (!string.IsNullOrWhiteSpace(_pi.Item.MediaUri) && File.Exists(_pi.Item.MediaUri))
            {
                contentNode.SetAttributeValue("mediaUri", _pi.Item.MediaUri);
            }

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
            //string xslText = _pi.AsParallel ? ReadFile("parallel.xslt") : ReadFile("single.xslt");
            string xslText = ReadFile("single.xslt");

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
