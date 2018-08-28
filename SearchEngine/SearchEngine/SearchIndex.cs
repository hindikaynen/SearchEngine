using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine
{
    public class SearchIndex : ISearchIndex, IQueryRunner
    {
        private readonly IAnalyzer _analyzer;
        private readonly IStore _store;

        public SearchIndex(IAnalyzer analyzer, IStore store)
        {
            if (analyzer == null)
                throw new ArgumentNullException(nameof(analyzer));
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            _analyzer = analyzer;
            _store = store;
        }

        public void AddDocument(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var newDocId = _store.NextDocId();
            foreach (var field in document.Fields.Where(x => x.Flags.HasFlag(FieldFlags.Stored)))
            {
                _store.SetStoredFieldValue(newDocId, field.Name, ReadFieldValue(field));
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
            if (term == null)
                throw new ArgumentNullException(nameof(term));

            var postings = _store.GetPostings(term.FieldName, term.Value);
            foreach (var docId in postings)
            {
                _store.RemoveDocument(docId);
            }
        }

        public List<long> Search(IQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.Run(this).ToList();
        }

        public string GetFieldValue(long docId, string fieldName)
        {
            return _store.GetStoredFieldValue(docId, fieldName);
        }

        private IEnumerable<string> GetTokens(Field field)
        {
            if (!field.Flags.HasFlag(FieldFlags.Analyzed))
                return new[] {ReadFieldValue(field)};

            return _analyzer.Analyze(field.OpenReader);
        }

        IEnumerable<long> IQueryRunner.Search(string fieldName, string value)
        {
            var result = new HashSet<long>();
            foreach (var token in AnalyzeQuery(value))
            {
                var postings = _store.GetPostings(fieldName, token);
                result.UnionWith(postings);
            }
            return result;
        }

        private IEnumerable<string> AnalyzeQuery(string query)
        {
            if (WildcardHelper.IsWildcardQuery(query))
                return _store.WildcardSearch(query);

            return _analyzer.Analyze(query.AsStreamReader);
        }

        private static string ReadFieldValue(Field field)
        {
            using (var reader = field.OpenReader())
            {
                return reader.ReadToEnd();
            }
        }

        public void Dispose()
        {
            _store.Dispose();
        }
    }
}
