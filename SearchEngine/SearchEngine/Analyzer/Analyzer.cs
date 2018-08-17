using System;
using System.Collections.Generic;
using System.IO;

namespace SearchEngine
{
    public abstract class Analyzer
    {
        protected abstract IEnumerable<string> Tokenize(StreamReader reader);

        protected abstract string TransformToken(string token);

        protected abstract bool IsStopWord(string token);

        public IEnumerable<string> Analyze(Func<StreamReader> getReader)
        {
            using (var reader = getReader())
            {
                foreach (var token in Tokenize(reader))
                {
                    var transformed = TransformToken(token);
                    if (!IsStopWord(transformed))
                        yield return transformed;
                }
            }
        }
    }
}
