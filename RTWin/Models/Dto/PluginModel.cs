using System;
using System.ComponentModel.DataAnnotations;
using Ninject;
using NPoco;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Dto
{
    public class PluginModel : BaseDtoModel
    {
        public long PluginId { get; set; }

        private string _name;
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

        public string Description { get; set; }
        public string Content { get; set; }
        public string UUID { get; set; }

        public Plugin ToPlugin()
        {
            var pluginService = App.Container.Get<PluginService>();
            var l = pluginService.FindOne(this.PluginId);

            if (l == null)
            {
                l = Plugin.NewPlugin();
            }

            l.Name = this.Name;
            l.Description = this.Description;
            l.Content = this.Content;

            return l;
        }
    }
}
