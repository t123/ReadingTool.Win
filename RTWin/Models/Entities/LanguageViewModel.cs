namespace RTWin.Models
{
    public class LanguageViewModel
    {
        public long LanguageId { get; set; }
        public string Name { get; set; }
        public int TotalItems { get; set; }
        public int TotalTerms { get; set; }
        public int TotalKnown { get; set; }
        public int TotalUnknown { get; set; }
    }
}