using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.MemoryStore
{
    class InvertedIndex : IDisposable
    {
        private const int DefaultCleanUpPeriod = 10000;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<Posting>>> _indexByFieldName = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<Posting>>>();
        private readonly DeletedDocs _deletedDocs = new DeletedDocs();
        private readonly CleanUpTimer _cleanUpTimer;

        public InvertedIndex(int cleanUpPeriod = DefaultCleanUpPeriod)
        {
            _cleanUpTimer = new CleanUpTimer(CleanUp, cleanUpPeriod);
        }

        public void AddPosting(string fieldName, string token, Posting posting)
        {
            var index = _indexByFieldName.GetOrAdd(fieldName, f => new ConcurrentDictionary<string, List<Posting>>());
            var postings = index.GetOrAdd(token, t => new List<Posting>());
            lock (postings)
            {
                postings.Add(posting);
            }
        }

        public IEnumerable<Posting> GetPostings(string fieldName, string token)
        {
            ConcurrentDictionary<string, List<Posting>> index;
            if (!_indexByFieldName.TryGetValue(fieldName, out index))
                return Enumerable.Empty<Posting>();
            List<Posting> postings;
            if (!index.TryGetValue(token, out postings))
                return Enumerable.Empty<Posting>();

            lock (postings)
            {
                return postings.Where(p => !_deletedDocs.Contains(p.DocId)).ToList();
            }
        }

        public void MarkAsDeleted(long docId)
        {
            _deletedDocs.Add(docId);
        }

        private void CleanUp()
        {
            var toCleanUp = _deletedDocs.GetCopy();
            foreach (var index in _indexByFieldName.Values)
            {
                foreach (var postings in index.Values)
                {
                    lock (postings)
                    {
                        postings.RemoveAll(p => toCleanUp.Contains(p.DocId));
                    }
                }
            }
            _deletedDocs.ExceptWith(toCleanUp);
        }

        public void Dispose()
        {
            _cleanUpTimer.Dispose();
            _deletedDocs.Dispose();
        }
    }
}
