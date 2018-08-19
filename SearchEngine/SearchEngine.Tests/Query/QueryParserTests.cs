using System.Linq;
using NUnit.Framework;

namespace SearchEngine.Tests
{
    [TestFixture]
    public class QueryParserTests
    {
        [Test]
        public void ShouldParseAndQuery()
        {
            var parser = new QueryParser("name", DefaultOperator.And);
            var query = parser.Parse("hell* world");
            Assert.IsInstanceOf<AndQuery>(query);
            var andQuery = (AndQuery) query;
            Assert.AreEqual(2, andQuery.Subqueries.Count);
            var terms = andQuery.Subqueries.Cast<TermQuery>().Select(x => x.Term).ToList();
            Assert.AreEqual("hell*", terms[0].Value);
            Assert.AreEqual("name", terms[0].FieldName);
            Assert.AreEqual("world", terms[1].Value);
            Assert.AreEqual("name", terms[1].FieldName);
        }

        [Test]
        public void ShouldParseOrQuery()
        {
            var parser = new QueryParser("name", DefaultOperator.Or);
            var query = parser.Parse("hell* world");
            Assert.IsInstanceOf<OrQuery>(query);
            var orQuery = (OrQuery)query;
            Assert.AreEqual(2, orQuery.Subqueries.Count);
            var terms = orQuery.Subqueries.Cast<TermQuery>().Select(x => x.Term).ToList();
            Assert.AreEqual("hell*", terms[0].Value);
            Assert.AreEqual("name", terms[0].FieldName);
            Assert.AreEqual("world", terms[1].Value);
            Assert.AreEqual("name", terms[1].FieldName);
        }
    }
}
