using System.Collections.Generic;

namespace SearchEngine
{
    public interface IQueryRunner
    {
        IEnumerable<long> Search(string fieldName, string value);
    }
}
