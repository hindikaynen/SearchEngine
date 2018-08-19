using System.Collections.Generic;
using System.Linq;

namespace SearchEngine
{
    public class AndQuery : IAggregateQuery
    {
        public AndQuery()
        {
            Subqueries = new List<IQuery>();
        }

        public List<IQuery> Subqueries { get; }

        public IEnumerable<long> Run(IQueryRunner runner)
        {
            return Subqueries.Select(x => x.Run(runner)).Aggregate((x, y) => x.Intersect(y));
        }
    }
}
