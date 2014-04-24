using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ninject;
using RTWin.Core.Enums;
using RTWin.Core.Extensions;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Dto
{
    public class LanguageModel : BaseDtoModel
    {
        private string _name;
        private string _termRegex;
        private string _sentenceRegex;
        public long LanguageId { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                const string field = "Name";
                if (string.IsNullOrWhiteSpace(value))
                {
                    _errors[field] = true;
                    throw new ValidationException();
                }

                _errors[field] = false;
                _name = value;
                OnPropertyChanged(field);
            }
        }

        public string LanguageCode { get; set; }
        public bool IsArchived { get; set; }

        public string TermRegex
        {
            get { return _termRegex; }
            set
            {
                const string field = "TermRegex";
                if (!value.IsValidRegex())
                {
                    _errors[field] = true;
                    throw new ValidationException();
                }

                _errors[field] = false;
                _termRegex = value;
                OnPropertyChanged(field);
            }
        }

        public string SentenceRegex
        {
            get { return _sentenceRegex; }
            set
            {
                const string field = "SentenceRegex";
                if (!value.IsValidRegex())
                {
                    _errors[field] = true;
                    throw new ValidationException();
                }

                _errors[field] = false;
                _sentenceRegex = value;
                OnPropertyChanged(field);
            }
        }

        public LanguageDirection Direction { get; set; }
        public IList<PluginLanguage> Plugins { get; set; }

        public LanguageModel()
        {
            Plugins = new List<PluginLanguage>();
        }

        public Language ToLanguage()
        {
            var languageService = App.Container.Get<LanguageService>();
            var l = languageService.FindOne(this.LanguageId);

            if (l == null)
            {
                l = Language.NewLanguage();
            }

            l.Name = this.Name;
            l.IsArchived = this.IsArchived;
            l.LanguageCode = this.LanguageCode;
            l.SentenceRegex = this.SentenceRegex;
            l.TermRegex = this.TermRegex;
            l.Direction = this.Direction;

            return l;
        }
    }
}
