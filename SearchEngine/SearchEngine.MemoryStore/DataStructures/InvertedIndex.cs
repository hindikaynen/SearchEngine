using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.MemoryStore
{
    class InvertedIndex : IDisposable
    {
        private const int DefaultCleanUpPeriod = 10000;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<long>>> _indexByFieldName = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<long>>>();
        private readonly DeletedDocs _deletedDocs = new DeletedDocs();
        private readonly CleanUpTimer _cleanUpTimer;

        public InvertedIndex(int cleanUpPeriod = DefaultCleanUpPeriod)
        {
            _cleanUpTimer = new CleanUpTimer(CleanUp, cleanUpPeriod);
        }

        public void AddPosting(string fieldName, string token, long docId)
        {
            var index = _indexByFieldName.GetOrAdd(fieldName, f => new ConcurrentDictionary<string, List<long>>());
            var postings = index.GetOrAdd(token, t => new List<long>());
            lock (postings)
            {
                postings.Add(docId);
            }
        }

        public IEnumerable<long> GetPostings(string fieldName, string token)
        {
            ConcurrentDictionary<string, List<long>> index;
            if (!_indexByFieldName.TryGetValue(fieldName, out index))
                return Enumerable.Empty<long>();
            List<long> postings;
            if (!index.TryGetValue(token, out postings))
                return Enumerable.Empty<long>();

            List<long> postingsCopy;
            lock (postings)
            {
                postingsCopy = postings.ToList();
            }
            return postingsCopy.Where(p => !_deletedDocs.Contains(p)).ToList();
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
                        postings.RemoveAll(p => toCleanUp.Contains(p));
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
