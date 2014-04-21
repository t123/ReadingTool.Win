using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPoco;

namespace RTWin.Entities
{
    [TableName("language")]
    [PrimaryKey("LanguageId")]
    public class Language
    {
        public const string TermRegex = "([a-zA-ZÀ-ÖØ-öø-ȳ\\'-]+)";
        public const string SentenceRegex = "[^\\.!\\?]+[\\.!\\?\n]+";

        public long LanguageId { get; set; }
        public string Name { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsArchived { get; set; }

        [Ignore]
        public LanguageSettings Settings
        {
            get { return JsonConvert.DeserializeObject<LanguageSettings>(JsonSettings ?? ""); }
            set { JsonSettings = JsonConvert.SerializeObject(value); }
        }

        public string LanguageCode { get; set; }
        public string JsonSettings { get; set; }
        public long UserId { get; set; }
    }
}
