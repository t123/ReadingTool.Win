using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTWin.Entities
{
    public class TermDictionary
    {
        public long Id { get; set; }
        public string LanguageCode { get; set; }
        public string Phrase { get; set; }
        public string BasePhrase { get; set; }
        public string Definition { get; set; }
    }
}
