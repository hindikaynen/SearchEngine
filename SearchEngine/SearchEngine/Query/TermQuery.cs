using System.Collections.Generic;

namespace SearchEngine
{
    public class TermQuery : IQuery
    {
        public Term Term { get; }

        public TermQuery(Term term)
        {
            Term = term;
        }

        public IEnumerable<long> Run(IQueryRunner runner)
        {
            return runner.Search(Term.FieldName, Term.Value);
        }
    }
}
