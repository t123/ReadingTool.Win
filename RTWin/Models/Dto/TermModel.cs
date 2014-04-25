using System;
using Ninject;
using NPoco;
using RTWin.Core.Enums;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Dto
{
    public class TermModel : BaseDtoModel
    {
        public long TermId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        private string _phrase;

        public string Phrase
        {
            get { return _phrase; }
            set
            {
                _phrase = value;
                LowerPhrase = value == null ? null : value.ToLowerInvariant();
            }
        }

        public string BasePhrase { get; set; }
        public string Definition { get; set; }
        public string Sentence { get; set; }
        public long LanguageId { get; set; }
        public long ItemSourceId { get; set; }
        public TermState State { get; set; }
        public string LowerPhrase { get; set; }
        public long UserId { get; set; }

        [Ignore]
        public string FullDefinition
        {
            get
            {
                string s = string.IsNullOrWhiteSpace(BasePhrase) ? "" : BasePhrase + "<br/>";
                if (!string.IsNullOrWhiteSpace(Definition)) s += Definition;
                return s;
            }
        }

        [ResultColumn]
        public string Language { get; set; }

        [ResultColumn]
        public string ItemSource { get; set; }

        public Term ToTerm()
        {
            var termService = App.Container.Get<TermService>();
            var l = termService.FindOne(this.LanguageId);

            if (l == null)
            {
                l = Term.NewTerm();
            }

            return l;
        }
    }
}