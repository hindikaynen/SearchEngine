using Moq;
using NUnit.Framework;
using SearchEngine.Analysis;
using SearchEngine.MemoryStore;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class SearchIndexTests
    {
        [Test]
        public void AddDocument()
        {
            var analyzer = new Mock<IAnalyzer>().SplitByWhitespace();
            var store = new Mock<IStore>();
            var docId = Utils.RandomInteger();
            store.Setup(x => x.NextDocId()).Returns(docId);

            var searchIndex = new SearchIndex(analyzer.Object, store.Object);
            var document = new Document();
            document.AddField(new StringField("name", "war and peace.txt", FieldFlags.Stored));
            document.AddField(new StringField("content", "hello world", FieldFlags.Analyzed));
            searchIndex.AddDocument(document);

            store.Verify(x => x.SetStoredFieldValue(docId, "name", "war and peace.txt"), Times.Once);
            store.Verify(x => x.SetStoredFieldValue(docId, "content", It.IsAny<string>()), Times.Never);

            store.Verify(x => x.AddPosting("name", "war and peace.txt", docId), Times.Once);
            store.Verify(x => x.AddPosting("content", "hello", docId), Times.Once);
            store.Verify(x => x.AddPosting("content", "world", docId), Times.Once);
        }

        [Test]
        public void RemoveDocument()
        {
            var analyzer = new Mock<IAnalyzer>().SplitByWhitespace();
            var store = new Mock<IStore>();
            var docId = Utils.RandomInteger();
            store.Setup(x => x.GetPostings("name", "hello")).Returns(new long[] {docId});
            var searchIndex = new SearchIndex(analyzer.Object, store.Object);
            var term = new Term("name", "hello");

            searchIndex.RemoveDocument(term);

            store.Verify(x => x.RemoveDocument(docId), Times.Once);
        }

        [Test]
        public void QueryRunnerSearch()
        {
            var analyzer = new Mock<IAnalyzer>();
            analyzer.Setup(x => x.TransformToken(It.IsAny<string>())).Returns((string s) => s);
            var store = new Mock<IStore>();
            store.Setup(x => x.WildcardSearch(It.IsAny<string>())).Returns((string s) => new[] {s});
            store.Setup(x => x.WildcardSearch("wor*")).Returns(new[] {"world", "worry"});
            IQueryRunner runner = new SearchIndex(analyzer.Object, store.Object);
            store.Setup(x => x.GetPostings("name", "hello")).Returns(new long[] { 1, 2});
            store.Setup(x => x.GetPostings("name", "world")).Returns(new long[] { 2, 3 });
            store.Setup(x => x.GetPostings("name", "worry")).Returns(new long[] { 3, 4 });

            var result = runner.Search("name", "wor*");

            CollectionAssert.AreEquivalent(new[] {2, 3, 4}, result);
        }

        [Test]
        public void GetFieldValue()
        {
            var docId = Utils.RandomInteger();
            var analyzer = new Mock<IAnalyzer>().SplitByWhitespace();
            var store = new Mock<IStore>();
            store.Setup(x => x.GetStoredFieldValue(docId, "name")).Returns("value");
            var searchIndex = new SearchIndex(analyzer.Object, store.Object);

            var value = searchIndex.GetFieldValue(docId, "name");

            Assert.AreEqual("value", value);
        }

        [Test]
        public void RealLife()
        {
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();
            var searchIndex = new SearchIndex(analyzer, store);

            var doc1 = new Document();
            doc1.AddField(new StringField("name", "name1", FieldFlags.Stored));
            doc1.AddField(new StringField("content", "hello world", FieldFlags.Analyzed));
            searchIndex.AddDocument(doc1);

            var doc2 = new Document();
            doc2.AddField(new StringField("name", "name2", FieldFlags.Stored));
            doc2.AddField(new StringField("content", "hi pretty world", FieldFlags.Analyzed));
            searchIndex.AddDocument(doc2);

            var doc3 = new Document();
            doc3.AddField(new StringField("name", "name3", FieldFlags.Stored));
            doc3.AddField(new StringField("content", "hell worm", FieldFlags.Analyzed));
            searchIndex.AddDocument(doc3);

            var query = new AndQuery();
            query.Subqueries.Add(new TermQuery(new Term("content", "hell*")));
            query.Subqueries.Add(new TermQuery(new Term("content", "wor*")));

            var docs = searchIndex.Search(query);
            Assert.AreEqual(2, docs.Count);

            var name1 = searchIndex.GetFieldValue(docs[0], "name");
            var name2 = searchIndex.GetFieldValue(docs[1], "name");
            CollectionAssert.AreEquivalent(new[] {"name1", "name3"}, new[] {name1, name2});

            searchIndex.RemoveDocument(new Term("name", "name1"));

            docs = searchIndex.Search(query);
            Assert.AreEqual(1, docs.Count);

            Assert.AreEqual("name3", searchIndex.GetFieldValue(docs[0], "name"));
        }
    }
}
