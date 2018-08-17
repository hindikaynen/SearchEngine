using NUnit.Framework;
using SearchEngine.Analysis;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class SimpleAnalyzerTests
    {
        [Test]
        public void ShouldLowercase()
        {
            var analyzer = new SimpleAnalyzer();
            string input = "HeLLo";
            var tokens = analyzer.Analyze(() => input.AsStreamReader());
            CollectionAssert.AreEqual(new[] { "hello" }, tokens);
        }

        [Test]
        public void ShouldReplaceYeYo()
        {
            var analyzer = new SimpleAnalyzer();
            string input = "дёготь";
            var tokens = analyzer.Analyze(() => input.AsStreamReader());
            CollectionAssert.AreEqual(new[] { "деготь" }, tokens);
        }

        [Test]
        public void ShouldAnalyze()
        {
            string input = "\r\n Лёгок         как на помине  ";
            var analyzer = new SimpleAnalyzer();
            var tokens = analyzer.Analyze(() => input.AsStreamReader());
            CollectionAssert.AreEqual(new[] {"легок", "помине"}, tokens);
        }
    }
}
