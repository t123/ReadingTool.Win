using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace RTWin.Entities
{
    [TableName("plugin")]
    [PrimaryKey("PluginId")]
    public class Plugin
    {
        public long PluginId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string UUID { get; set; }

        [Ignore]
        public Guid UuidAsGuid
        {
            get
            {
                return new Guid(UUID);
            }
        }
    }

    [TableName("language_plugin")]
    [PrimaryKey("LanguageId, PluginId")]
    public class LanguagePlugin
    {
        public long LanguageId { get; set; }
        public long PluginId { get; set; }
    }

    [TableName("plugin_storage")]
    [PrimaryKey("Id")]
    public class PluginStorage
    {
        public long Id { get; set; }
        public string UUID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
