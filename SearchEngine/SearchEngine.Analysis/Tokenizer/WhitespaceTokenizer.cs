namespace SearchEngine.Analysis
{
    public class WhitespaceTokenizer : CharTokenizer
    {
        protected override bool IsTokenChar(char c)
        {
            return !char.IsWhiteSpace(c);
        }
    }
}
