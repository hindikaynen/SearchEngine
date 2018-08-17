using System;

namespace SearchEngine
{
    public enum DefaultOperator
    {
        And,
        Or
    }

    public class QueryParser
    {
        public QueryParser(string defaultFieldName, DefaultOperator defaultOperator)
        {
            DefaultFieldName = defaultFieldName;
            DefaultOperator = defaultOperator;
        }

        public string DefaultFieldName { get; }
        public DefaultOperator DefaultOperator { get; }

        public IQuery Parse(string query)
        {
            var aggregateQuery = CreateAggregateQuery();
            foreach (var token in query.Split(null))
            {
                var termQuery = new TermQuery(new Term(DefaultFieldName, token));
                aggregateQuery.Subqueries.Add(termQuery);
            }
            return aggregateQuery;
        }

        private IAggregateQuery CreateAggregateQuery()
        {
            switch (DefaultOperator)
            {
                case DefaultOperator.And:
                    return new AndQuery();
                case DefaultOperator.Or:
                    return new OrQuery();
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
