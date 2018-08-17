using System;
using System.Collections.Generic;
using System.IO;

namespace SearchEngine
{
    public interface IAnalyzer
    {
        IEnumerable<string> Analyze(Func<StreamReader> getReader);
    }
}
