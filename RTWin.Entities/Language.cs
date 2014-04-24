using System;
using Newtonsoft.Json;
using NPoco;
using RTWin.Core.Enums;

namespace RTWin.Entities
{
    [TableName("language")]
    [PrimaryKey("LanguageId")]
    public class Language
    {
        public const string TERM_REGEX = "([a-zA-ZÀ-ÖØ-öø-ȳ\\'-]+)";
        public const string SENTENCE_REGEX = "[^\\.!\\?]+[\\.!\\?\n]+";

        public long LanguageId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsArchived { get; set; }
        public string LanguageCode { get; set; }
        public long UserId { get; set; }
        public string TermRegex { get; set; }
        public string SentenceRegex { get; set; }
        public LanguageDirection Direction { get; set; }

        public static Language NewLanguage()
        {
            return new Language()
            {
                Name = "New language",
                Direction = LanguageDirection.LeftToRight,
                SentenceRegex = SENTENCE_REGEX,
                TermRegex = TERM_REGEX,
                IsArchived = false,
                LanguageCode = "--"
            };
        }
    }
}
