using NUnit.Framework;
using SearchEngine.MemoryStore;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class DeletedDocsTests
    {
        [Test]
        public void ShouldWork()
        {
            var docs = new DeletedDocs();
            docs.Add(1);
            Assert.IsTrue(docs.Contains(1));
            Assert.IsFalse(docs.Contains(2));
            docs.Add(2);
            docs.Add(3);
            docs.ExceptWith(new long[] {1, 3});
            Assert.IsFalse(docs.Contains(1));
            Assert.IsTrue(docs.Contains(2));
            Assert.IsFalse(docs.Contains(3));
            docs.Add(4);
            CollectionAssert.AreEquivalent(new[] {2, 4}, docs.GetCopy());
            docs.Dispose();
        }
    }
}
