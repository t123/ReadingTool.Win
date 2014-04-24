using System;
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

        public static Plugin NewPlugin()
        {
            return new Plugin()
            {
                Name = "New Plugin",
                Description = string.Format("Created {0:dd/MM/yyyy}", DateTime.Now),
                Content = "//your Javascript here",
                UUID = Guid.NewGuid().ToString()
            };
        }
    }
}
