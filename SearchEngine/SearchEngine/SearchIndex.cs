using System.Collections.Generic;
using System.Linq;

namespace SearchEngine
{
    public class SearchIndex
    {
        private readonly IAnalyzer _analyzer;
        private readonly IStore _store;

        public SearchIndex(IAnalyzer analyzer, IStore store)
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
                    _store.AddPosting(field.Name, token, new Posting(newDocId));
                }
            }
        }

        public void RemoveDocument(Term term)
        {
            var postings = _store.GetPostings(term.FieldName, term.Value);
            foreach (var posting in postings)
            {
                _store.RemoveDocument(posting.DocId);
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
    }
}
