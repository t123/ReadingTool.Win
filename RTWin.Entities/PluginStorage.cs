using NPoco;

namespace RTWin.Entities
{
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