using System.IO;
using System.Text;
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
            string output;
            Assert.IsTrue(analyzer.ProcessToken(input, out output));
            Assert.AreEqual("hello", output);
        }

        [Test]
        public void ShouldReplaceYeYo()
        {
            var analyzer = new SimpleAnalyzer();
            string input = "дёготь";
            string output;
            Assert.IsTrue(analyzer.ProcessToken(input, out output));
            Assert.AreEqual("деготь", output);
        }

        [Test]
        public void ShouldAnalyze()
        {
            string input = "\r\n Лёгок         как на помине  ";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                var analyzer = new SimpleAnalyzer();
                var tokens = analyzer.Analyze(() => new StreamReader(stream));
                CollectionAssert.AreEqual(new[] {"легок", "помине"}, tokens);
            }
        }
    }
}
