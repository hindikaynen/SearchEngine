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
            using (var streamReader = input.AsStreamReader())
            {
                var tokens = tokenizer.Tokenize(streamReader);
                CollectionAssert.AreEquivalent(new[] { "hello", "pretty", "world" }, tokens);
            }
        }

        [Test]
        public void ShouldTokenizeLongText()
        {
            var word1 = Utils.RandomWord(1000, 1000);
            var word2 = Utils.RandomWord(1000, 1000);
            var word3 = Utils.RandomWord(1000, 1000);
            var input = $" \r\n   {word1}  {word2} \r\n {word3} ";
            var tokenizer = new WhitespaceTokenizer();
            using (var streamReader = input.AsStreamReader())
            {
                var tokens = tokenizer.Tokenize(streamReader);
                CollectionAssert.AreEquivalent(new[] { word1, word2, word3 }, tokens);
            }
        }
    }
}
