using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace RTWin.Services
{
    public abstract class BaseParserService : IParserService
    {
        protected string _xsltFile;

        public abstract ParserOutput Parse(ParserInput pi);

        protected void AddDataToContentNode(ParserInput pi, XElement contentNode)
        {
            contentNode.SetAttributeValue("isParallel", pi.AsParallel);
            contentNode.SetAttributeValue("signalR", pi.SignalREndPoint);
            contentNode.SetAttributeValue("webApi", pi.WebApiEndPoint);
            contentNode.SetAttributeValue("isParallel", pi.AsParallel);
            contentNode.SetAttributeValue("collectionName", pi.Item.CollectionName);
            contentNode.SetAttributeValue("collectionNo", pi.Item.CollectionNo);
            contentNode.SetAttributeValue("dateCreated", pi.Item.DateCreated);
            contentNode.SetAttributeValue("dateModified", pi.Item.DateModified);
            contentNode.SetAttributeValue("lastRead", pi.Item.LastRead);
            contentNode.SetAttributeValue("l1Title", pi.Item.L1Title);
            contentNode.SetAttributeValue("l2Title", pi.Item.L2Title);
            contentNode.SetAttributeValue("l1Id", pi.Language1.LanguageId);
            contentNode.SetAttributeValue("itemType", pi.Item.ItemType.ToString().ToLowerInvariant());
            contentNode.SetAttributeValue("itemId", pi.Item.ItemId);
            contentNode.SetAttributeValue("l1Direction", pi.Language1.Settings.Direction);
            contentNode.SetAttributeValue("l2Direction", pi.AsParallel ? pi.Language2.Settings.Direction.ToString() : "");
            contentNode.SetAttributeValue("l1Code", pi.Language1.LanguageCode);
            contentNode.SetAttributeValue("l2Code", pi.Language2 == null ? "" : pi.Language2.LanguageCode);

            if (!string.IsNullOrWhiteSpace(pi.Item.MediaUri) && File.Exists(pi.Item.MediaUri))
            {
                contentNode.SetAttributeValue("mediaUri", pi.Item.MediaUri);
            }
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