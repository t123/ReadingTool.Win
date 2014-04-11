namespace RTWin.Entities
{
    public class LanguageSettings
    {
        public string TermRegex { get; set; }
        public string SentenceRegex { get; set; }
        public Direction Direction { get; set; }
        public bool DisplaySpaces { get; set; }
        public bool PauseOnModal { get; set; }
    }
}