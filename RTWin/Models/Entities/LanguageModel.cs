using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models
{
    public class LanguageModel
    {
        public long LanguageId { get; set; }
        public string Name { get; set; }
        public string LanguageCode { get; set; }
        public bool IsArchived { get; set; }
        public string TermRegex { get; set; }
        public string SentenceRegex { get; set; }
        public Direction Direction { get; set; }

        public IList<PluginLanguage> Plugins { get; set; }
        public LanguageModel()
        {
            Plugins = new List<PluginLanguage>();
        }
    }
}
