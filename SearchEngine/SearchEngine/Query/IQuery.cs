using System.Collections.Generic;

namespace SearchEngine
{
    public interface IQuery
    {
        IEnumerable<long> Run(IQueryRunner runner);
    }
}
