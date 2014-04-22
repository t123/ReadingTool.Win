using NPoco;

namespace RTWin.Entities
{
    [TableName("languagecode")]
    //[PrimaryKey("Code")]
    public class LanguageCode
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }
}