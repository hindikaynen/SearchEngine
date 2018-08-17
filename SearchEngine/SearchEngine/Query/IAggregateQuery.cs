using System.Collections.Generic;

namespace SearchEngine
{
    public interface IAggregateQuery : IQuery
    {
        List<IQuery> Subqueries { get; }
    }
}