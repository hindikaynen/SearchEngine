using System.Collections.Generic;
using System.Linq;

namespace SearchEngine
{
    public class SearchIndex : IQueryRunner
    {
        private readonly Analyzer _analyzer;
        private readonly IStore _store;

        public SearchIndex(Analyzer analyzer, IStore store)
        {
            _analyzer = analyzer;
            _store = store;
        }

        public void AddDocument(Document document)
        {
            var newDocId = _store.NextDocId();
            foreach (var field in document.Fields.Where(x => x.Flags.HasFlag(FieldFlags.Stored)))
            {
                _store.SetStoredFieldValue(newDocId, field.Name, field.ToString());
            }
            foreach (var field in document.Fields)
            {
                foreach (var token in GetTokens(field))
                {
                    _store.AddPosting(field.Name, token, newDocId);
                }
            }
        }

        public void RemoveDocument(Term term)
        {
            var postings = _store.GetPostings(term.FieldName, term.Value);
            foreach (var docId in postings)
            {
                _store.RemoveDocument(docId);
            }
        }

        public void UpdateDocument(Term term, Document document)
        {
            RemoveDocument(term);
            AddDocument(document);
        }

        private IEnumerable<string> GetTokens(Field field)
        {
            if (!field.Flags.HasFlag(FieldFlags.Analyzed))
                return new[] {field.ToString()};

            return _analyzer.Analyze(field.OpenReader);
        }

        IEnumerable<long> IQueryRunner.Search(string fieldName, string value)
        {
            var tokens = _analyzer.Analyze(value.AsStreamReader);
            HashSet<long> result = new HashSet<long>();
            foreach (var token in tokens)
            {
                var matches = _store.WildcardSearch(token);
                foreach (var match in matches)
                {
                    var postings = _store.GetPostings(fieldName, match);
                    result.UnionWith(postings);
                }
            }
            return result;
        }
    }
}
