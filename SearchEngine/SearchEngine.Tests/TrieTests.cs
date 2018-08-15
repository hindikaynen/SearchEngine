using System;
using System.Threading.Tasks;
using NUnit.Framework;
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
            Assert.Throws<ArgumentException>(() => trie.SearchByPrefix(null));
            Assert.Throws<ArgumentException>(() => trie.SearchByPrefix(string.Empty));
        }

        [Test]
        public void ShouldSearchByPrefix()
        {
            var trie = new Trie();
            trie.Add("ABC");
            trie.Add("ABCD");
            trie.Add("ABCDE");
            trie.Add("ABCDEF");
            trie.Add("ABDEF");

            var found = trie.SearchByPrefix("ABCD");
            CollectionAssert.AreEquivalent(new[] {"ABCD", "ABCDE", "ABCDEF"}, found);
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
            var found = trie.SearchByPrefix("ABC");
            CollectionAssert.Contains(found, "ABCD");
        }
    }
}
