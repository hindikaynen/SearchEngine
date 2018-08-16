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
                    string processedToken;
                    if (ProcessToken(token, out processedToken))
                        yield return processedToken;
                }
            }
        }

        public bool ProcessToken(string token, out string result)
        {
            var transformed = TransformToken(token);
            if (!IsStopWord(transformed))
            {
                result = transformed;
                return true;
            }
            result = default(string);
            return false;
        }
    }
}
