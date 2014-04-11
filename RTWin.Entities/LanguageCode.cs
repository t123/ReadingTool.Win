using NPoco;

namespace RTWin.Entities
{
    [TableName("languagecode")]
    [PrimaryKey("LanguageCodeId")]
    public class LanguageCode
    {
        public long LanguageCodeId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}