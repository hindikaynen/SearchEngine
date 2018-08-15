﻿using System.Collections.Generic;
using System.Threading;

namespace SearchEngine.MemoryStore
{
    public class MemoryStore : IStore
    {
        private readonly InvertedIndex _invertedIndex = new InvertedIndex();
        private readonly FieldStore _fieldStore = new FieldStore();
        private readonly Trie _trie = new Trie();
        private long _lastDocId = -1;

        public long NextDocId()
        {
            return Interlocked.Increment(ref _lastDocId);
        }

        public void AddPosting(string fieldName, string token, Posting posting)
        {
            _invertedIndex.AddPosting(fieldName, token, posting);
            _trie.Add(token);
        }

        public IEnumerable<Posting> GetPostings(string fieldName, string token)
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

        public IEnumerable<string> SearchByPrefix(string prefix)
        {
            return _trie.SearchByPrefix(prefix);
        }

        public void Dispose()
        {
            _invertedIndex.Dispose();
        }
    }
}
