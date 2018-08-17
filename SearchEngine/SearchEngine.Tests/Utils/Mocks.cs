using System;
using System.IO;
using Moq;

namespace SearchEngine.Tests
{
    public static class Mocks
    {
        public static Mock<IAnalyzer> SplitByWhitespace(this Mock<IAnalyzer> analyzer)
        {
            analyzer.Setup(x => x.Analyze(It.IsAny<Func<StreamReader>>()))
                .Returns((Func<StreamReader> getReader) =>
                {
                    using (var reader = getReader())
                    {
                        return reader.ReadToEnd().Split(null);
                    }
                });
            return analyzer;
        }
    }
}
