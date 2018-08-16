using System.IO;
using System.Text;
using NUnit.Framework;
using SearchEngine.Analysis;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class WhitespaceTokenizerTests
    {
        [Test]
        public void ShouldTokenize()
        {
            var input = "    hello  pretty \r\n world   ";
            var tokenizer = new WhitespaceTokenizer();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using (var reader = new StreamReader(stream))
            {
                var tokens = tokenizer.Tokenize(reader);
                CollectionAssert.AreEquivalent(new[] {"hello", "pretty", "world"}, tokens);
            }
        }
    }
}
