using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SearchEngine.Analysis;
using SearchEngine.MemoryStore;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class TrieTests
    {
        [Test]
        public void ShouldThrowIfNullOrEmptyAdded()
        {
            var trie = new Trie();
            Assert.Throws<ArgumentException>(() => trie.Add(null));
            Assert.Throws<ArgumentException>(() => trie.Add(string.Empty));
        }

        [Test]
        public void ShouldThrowIfNullOrEmptySearchByPrefix()
        {
            var trie = new Trie();
            Assert.Throws<ArgumentException>(() => trie.WildcardSearch(null));
            Assert.Throws<ArgumentException>(() => trie.WildcardSearch(string.Empty));
        }

        [Test]
        public void ShouldSearchWildcardStarAtTheEnd()
        {
            var trie = new Trie();
            trie.Add("ABC");
            trie.Add("ABCD");
            trie.Add("ABCDE");
            trie.Add("ABCDEF");
            trie.Add("ABDEF");

            var found = trie.WildcardSearch("ABCD*");
            CollectionAssert.AreEquivalent(new[] {"ABCD", "ABCDE", "ABCDEF"}, found);
        }

        [Test]
        public void ShouldSearchWildcardStarInTheMiddle()
        {
            var trie = new Trie();
            trie.Add("ABC");
            trie.Add("ABCD");
            trie.Add("ABCDE");
            trie.Add("ABCDEF");
            trie.Add("ABDEF");

            var found = trie.WildcardSearch("AB*EF");
            CollectionAssert.AreEquivalent(new[] { "ABDEF", "ABCDEF" }, found);
        }

        [Test]
        public void ShouldSearchWildcardQuestingMark()
        {
            var trie = new Trie();
            trie.Add("ABC");
            trie.Add("ABCD");
            trie.Add("ABCDE");
            trie.Add("ABDE");
            trie.Add("ABFDE");
            trie.Add("ABCDEF");
            trie.Add("ABDEF");

            var found = trie.WildcardSearch("AB?DE");
            CollectionAssert.AreEquivalent(new[] { "ABCDE", "ABFDE" }, found);
        }

        [Test]
        public void ShouldIndexInParallel()
        {
            var trie = new Trie();
            trie.Add("ABCD");
            Parallel.For(0, 100000, i =>
            {
                trie.Add(Utils.RandomWord(10));
            });
            var found = trie.WildcardSearch("ABC*");
            CollectionAssert.Contains(found, "ABCD");
        }

        [Test]
        public void WarAndPeace()
        {
            var trie = new Trie();
            var filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDocs", "warandpeace.txt");
            var tokenizer = new LetterTokenizer();

            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                foreach (var token in tokenizer.Tokenize(reader))
                {
                    trie.Add(token.ToLower());
                }
            }
            
            var found = trie.WildcardSearch("при?ет");
            CollectionAssert.AreEquivalent(new[] {"придет", "примет"}, found);

            found = trie.WildcardSearch("здр*в?й*");
            CollectionAssert.AreEquivalent(new[] { "здравый", "здравствуй", "здравствуйте" }, found);
        }
    }
}
