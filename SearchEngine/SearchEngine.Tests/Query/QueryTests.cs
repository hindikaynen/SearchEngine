using System.Linq;
using Moq;
using NUnit.Framework;

namespace SearchEngine.Tests.Query
{
    [TestFixture]
    public class QueryTests
    {
        [Test]
        public void TermQueryRun()
        {
            var postings = new long[] { Utils.RandomInteger()};

            var runner = new Mock<IQueryRunner>();
            runner.Setup(x => x.Search("field", "value")).Returns(postings);

            var query = new TermQuery(new Term("field", "value"));
            CollectionAssert.AreEqual(postings, query.Run(runner.Object));
        }

        [Test]
        public void AndQueryRun()
        {
            var postings1 = new long[] { 1, 2, 3, 4 };
            var postings2 = new long[] { 3, 4, 5, 6 };
            var runner = new Mock<IQueryRunner>(MockBehavior.Strict);
            var query1 = new Mock<IQuery>();
            query1.Setup(x => x.Run(runner.Object)).Returns(postings1);
            var query2 = new Mock<IQuery>();
            query2.Setup(x => x.Run(runner.Object)).Returns(postings2);

            var andQuery = new AndQuery();
            andQuery.Subqueries.Add(query1.Object);
            andQuery.Subqueries.Add(query2.Object);

            CollectionAssert.AreEquivalent(new[] {3, 4}, andQuery.Run(runner.Object));
        }

        [Test]
        public void OrQueryRun()
        {
            var postings1 = new long[] { 1, 2, 3, 4 };
            var postings2 = new long[] { 3, 4, 5, 6 };
            var runner = new Mock<IQueryRunner>(MockBehavior.Strict);
            var query1 = new Mock<IQuery>();
            query1.Setup(x => x.Run(runner.Object)).Returns(postings1);
            var query2 = new Mock<IQuery>();
            query2.Setup(x => x.Run(runner.Object)).Returns(postings2);

            var orQuery = new OrQuery();
            orQuery.Subqueries.Add(query1.Object);
            orQuery.Subqueries.Add(query2.Object);

            CollectionAssert.AreEquivalent(new[] { 1, 2, 3, 4, 5, 6 }, orQuery.Run(runner.Object));
        }
    }
}
