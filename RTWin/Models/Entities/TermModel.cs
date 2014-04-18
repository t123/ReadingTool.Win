using System;

namespace RTWin.Models
{
    public class TermModel
    {
        public long TermId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Phrase { get; set; }
        public string Language { get; set; }
        public string State { get; set; }
        public string Sentence { get; set; }
        public string BasePhrase { get; set; }
        public string Definition { get; set; }
    }
}