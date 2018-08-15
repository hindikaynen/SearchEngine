using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SearchEngine.MemoryStore
{
    class InvertedIndex : IDisposable
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<Posting>>> _indexByFieldName = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<Posting>>>();
        private readonly HashSet<long> _deletedDocs = new HashSet<long>();
        private readonly ReaderWriterLockSlim _deletedDocsLock = new ReaderWriterLockSlim();

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

            List<Posting> result;
            lock (postings)
            {
                result = postings.ToList();
            }

            _deletedDocsLock.EnterReadLock();
            try
            {
                result.RemoveAll(p => _deletedDocs.Contains(p.DocId));
            }
            finally
            {
                _deletedDocsLock.ExitReadLock();
            }
            return result;
        }

        public void MarkAsDeleted(long docId)
        {
            _deletedDocsLock.EnterWriteLock();
            try
            {
                _deletedDocs.Add(docId);
            }
            finally
            {
                _deletedDocsLock.ExitWriteLock();
            }
        }

        public void CleanUp()
        {
            var toCleanUp = new HashSet<long>(_deletedDocs);
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
            _deletedDocsLock.EnterWriteLock();
            try
            {
                _deletedDocs.ExceptWith(toCleanUp);
            }
            finally
            {
                _deletedDocsLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _deletedDocsLock.Dispose();
        }
    }
}
