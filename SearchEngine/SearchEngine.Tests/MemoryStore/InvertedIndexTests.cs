using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SearchEngine.MemoryStore;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class InvertedIndexTests
    {
        [Test]
        public void ShouldReturnEmptyIfNotFound()
        {
            var index = new InvertedIndex();
            CollectionAssert.IsEmpty(index.GetPostings(Utils.RandomString(), Utils.RandomString()));

            var fieldName = Utils.RandomString();
            index.AddPosting(fieldName, Utils.RandomString(), Utils.RandomInteger());

            CollectionAssert.IsEmpty(index.GetPostings(fieldName, Utils.RandomString()));
        }

        [Test]
        public void ShouldGetPostings()
        {
            var index = new InvertedIndex();
            var fieldName = Utils.RandomString();
            var value1 = Utils.RandomString();
            var value2 = Utils.RandomString();
            var docId1 = Utils.RandomInteger();
            var docId2 = Utils.RandomInteger();
            var docId3 = Utils.RandomInteger();

            index.AddPosting(fieldName, value1, docId1);
            index.AddPosting(fieldName, value2, docId2);
            index.AddPosting(fieldName, value2, docId3);

            var actual1 = index.GetPostings(fieldName, value1).ToList();
            Assert.AreEqual(1, actual1.Count);
            Assert.AreEqual(docId1, actual1[0]);

            var actual2 = index.GetPostings(fieldName, value2).ToList();
            Assert.AreEqual(2, actual2.Count);
            CollectionAssert.AreEquivalent(new[] {docId2, docId3 }, actual2);
        }

        [Test]
        public void ShouldMarkAsDeleted()
        {
            var index = new InvertedIndex();
            var fieldName = Utils.RandomString();
            var value1 = Utils.RandomString();
            var value2 = Utils.RandomString();
            var docId1 = Utils.RandomInteger();
            var docId2 = Utils.RandomInteger();
            var docId3 = Utils.RandomInteger();

            index.AddPosting(fieldName, value1, docId1);
            index.AddPosting(fieldName, value2, docId2);
            index.AddPosting(fieldName, value2, docId3);
            index.MarkAsDeleted(docId2);

            var actual1 = index.GetPostings(fieldName, value1).ToList();
            Assert.AreEqual(1, actual1.Count);
            Assert.AreEqual(docId1, actual1[0]);

            var actual2 = index.GetPostings(fieldName, value2).ToList();
            Assert.AreEqual(1, actual2.Count);
            Assert.AreEqual(docId3, actual2[0]);
        }

        [Test]
        public void ShouldAddPostingsInParallel()
        {
            const int count = 1000;

            long currentDocId = -1;
            var fieldNames = Enumerable.Repeat(0, count).Select(x => Utils.RandomString()).ToList();
            var values = Enumerable.Repeat(0, count).Select(x => Utils.RandomString()).ToList();
            var index = new InvertedIndex(cleanUpPeriod: 1);
            Parallel.For(0, count, i =>
            {
                var field = Utils.RandomElement(fieldNames);
                var value = Utils.RandomElement(values);
                index.AddPosting(field, value, Interlocked.Increment(ref currentDocId));
            });

            var docsCount = 0;
            foreach (var fieldName in fieldNames)
            {
                foreach (var value in values)
                {
                    docsCount += index.GetPostings(fieldName, value).Count();
                }
            }

            Assert.AreEqual(count, docsCount);
        }
    }
}
