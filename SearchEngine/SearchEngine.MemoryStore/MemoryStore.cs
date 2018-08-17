using System.Collections.Generic;
using System.Threading;

namespace SearchEngine.MemoryStore
{
    public class InMemoryStore : IStore
    {
        private readonly InvertedIndex _invertedIndex = new InvertedIndex();
        private readonly FieldStore _fieldStore = new FieldStore();
        private readonly Trie _trie = new Trie();
        private long _lastDocId = -1;

        public long NextDocId()
        {
            return Interlocked.Increment(ref _lastDocId);
        }

        public void AddPosting(string fieldName, string token, long docId)
        {
            _invertedIndex.AddPosting(fieldName, token, docId);
            _trie.Add(token);
        }

        public IEnumerable<long> GetPostings(string fieldName, string token)
        {
            return _invertedIndex.GetPostings(fieldName, token);
        }

        public void RemoveDocument(long docId)
        {
            _invertedIndex.MarkAsDeleted(docId);
        }

        public void SetStoredFieldValue(long docId, string fieldName, string value)
        {
            _fieldStore.SetValue(docId, fieldName, value);
        }

        public string GetStoredFieldValue(long docId, string fieldName)
        {
            return _fieldStore.GetValue(docId, fieldName);
        }

        public IEnumerable<string> WildcardSearch(string pattern)
        {
            return _trie.WildcardSearch(pattern);
        }

        public void Dispose()
        {
            _invertedIndex.Dispose();
        }
    }
}
