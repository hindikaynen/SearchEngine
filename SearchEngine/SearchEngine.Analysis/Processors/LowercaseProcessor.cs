namespace SearchEngine.Analysis
{
    public class LowercaseProcessor : IProcessor
    {
        public string ProcessToken(string token)
        {
            return token.ToLower();
        }
    }
}
