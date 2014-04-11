﻿using System;
using System.Collections.Generic;
using NPoco;

namespace RTWin.Entities
{
    [TableName("term")]
    [PrimaryKey("TermId")]
    public class Term
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
    }
}