using System.Collections.Generic;
using System.IO;

namespace SearchEngine.Analysis
{
    public class SimpleAnalyzer : Analyzer
    {
        private readonly LetterOrDigitTokenizer _tokenizer = new LetterOrDigitTokenizer();
        private readonly LowercaseProcessor _lowercaseProcessor = new LowercaseProcessor();
        private readonly RussianYeYoProcessor _russianYeYoProcessor = new RussianYeYoProcessor();

        protected override IEnumerable<string> Tokenize(StreamReader reader)
        {
            return _tokenizer.Tokenize(reader);
        }

        protected override string TransformToken(string token)
        {
            token = _lowercaseProcessor.ProcessToken(token);
            token = _russianYeYoProcessor.ProcessToken(token);
            return token;
        }

        protected override bool IsStopWord(string token)
        {
            return RussianStopWordFilter.IsStopWord(token) || EnglishStopWordFilter.IsStopWord(token);
        }
    }
}
