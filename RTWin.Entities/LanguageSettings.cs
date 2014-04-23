using RTWin.Core.Enums;

namespace RTWin.Entities
{
    public class LanguageSettings
    {
        public string TermRegex { get; set; }
        public string SentenceRegex { get; set; }
        public LanguageDirection LanguageDirection { get; set; }
    }
}