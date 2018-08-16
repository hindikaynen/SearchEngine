namespace SearchEngine.Analysis
{
    public class RussianYeYoProcessor : IProcessor
    {
        public string ProcessToken(string token)
        {
            return token.Replace("ё", "е");
        }
    }
}
