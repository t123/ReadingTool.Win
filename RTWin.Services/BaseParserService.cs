using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using RTWin.Entities;

namespace RTWin.Services
{
    public abstract class BaseParserService : IParserService
    {
        protected string _xsltFile;
        protected ParserInput _pi;
        protected ParserOutput _po;

        public abstract ParserOutput Parse(ParserInput pi);

        protected void AddDataToContentNode(XElement contentNode)
        {
            contentNode.SetAttributeValue("isParallel", _pi.AsParallel);
            contentNode.SetAttributeValue("signalR", _pi.SignalREndPoint);
            contentNode.SetAttributeValue("webA_pi", _pi.WebApiEndPoint);
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
            contentNode.SetAttributeValue("l1Direction", _pi.Language1.Settings.Direction);
            contentNode.SetAttributeValue("l2Direction", _pi.AsParallel ? _pi.Language2.Settings.Direction.ToString() : "");
            contentNode.SetAttributeValue("l1Code", _pi.Language1.LanguageCode);
            contentNode.SetAttributeValue("l2Code", _pi.Language2 == null ? "" : _pi.Language2.LanguageCode);

            if (!string.IsNullOrWhiteSpace(_pi.Item.MediaUri) && File.Exists(_pi.Item.MediaUri))
            {
                contentNode.SetAttributeValue("mediaUri", _pi.Item.MediaUri);
            }
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

            return termNode;
        }

        protected void WriteFile(string filename, string content)
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

        protected string ReadFile(string filename)
        {
            string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", filename);

            using (StreamReader sr = new StreamReader(file, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        protected string ApplyTransform(XDocument document, string filename)
        {
            string xslText = ReadFile(filename);

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