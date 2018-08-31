using System;
using System.Collections.Generic;
using System.IO;

namespace SearchEngine.Analysis
{
    public abstract class Analyzer : IAnalyzer
    {
        public abstract string TransformToken(string token);
        protected abstract IEnumerable<string> Tokenize(StreamReader reader);
        protected abstract bool IsStopWord(string token);

        public IEnumerable<string> Analyze(Func<StreamReader> getReader)
        {
            {
                using (var reader = getReader())
                {
                    foreach (var token in Tokenize(reader))
                    {
                        var transformed = TransformToken(token);
                        if (!IsStopWord(token))
                            yield return token;

                        if (!IsStopWord(transformed))
                            yield return transformed;
                    }
                }
            }
        }
    }
}
