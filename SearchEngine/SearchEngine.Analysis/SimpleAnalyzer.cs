using System;
using System.Collections.Generic;
using System.IO;

namespace SearchEngine.Analysis
{
    public class SimpleAnalyzer : Analyzer
    {
        private readonly WhitespaceTokenizer _tokenizer = new WhitespaceTokenizer();
        private readonly LowercaseProcessor _lowercaseProcessor = new LowercaseProcessor();
        private readonly RussianYeYoProcessor _russianYeYoProcessor = new RussianYeYoProcessor();

        public override IEnumerable<string> Tokenize(StreamReader reader)
        {
            return _tokenizer.Tokenize(reader);
        }

        public override string ProcessToken(string token)
        {
            token = _lowercaseProcessor.ProcessToken(token);
            token = _russianYeYoProcessor.ProcessToken(token);
            return token;
        }

        public override bool IsStopWord(string token)
        {
            return RussianStopWordFilter.IsStopWord(token) || EnglishStopWordFilter.IsStopWord(token);
        }
    }
}
