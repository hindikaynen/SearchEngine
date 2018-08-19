namespace SearchEngine.Analysis
{
    class LetterOrDigitTokenizer : CharTokenizer
    {
        protected override bool IsTokenChar(char c)
        {
            return char.IsLetterOrDigit(c);
        }
    }
}
