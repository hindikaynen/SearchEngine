namespace SearchEngine.Analysis
{
    class LetterTokenizer : CharTokenizer
    {
        protected override bool IsTokenChar(char c)
        {
            return char.IsLetter(c);
        }
    }
}
