using System;
using System.Collections.Generic;
using System.IO;

namespace SearchEngine
{
    public abstract class BaseAnalyzer : IAnalyzer
    {
        public abstract IEnumerable<string> Tokenize(StreamReader reader);

        public abstract string ProcessToken(string token);

        public abstract bool IsStopWord(string token);

        IEnumerable<string> IAnalyzer.Analyze(Func<StreamReader> getReader)
        {
            using (var reader = getReader())
            {
                foreach (var token in Tokenize(reader))
                {
                    var processToken = ProcessToken(token);
                    if (!IsStopWord(processToken))
                        yield return processToken;
                }
            }
        }
    }
}
