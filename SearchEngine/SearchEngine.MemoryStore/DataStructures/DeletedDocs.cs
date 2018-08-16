using System;
using System.Collections.Generic;
using System.Threading;

namespace SearchEngine.MemoryStore
{
    class DeletedDocs : IDisposable
    {
        private readonly HashSet<long> _deletedDocs = new HashSet<long>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Add(long docId)
        {
            _lock.EnterWriteLock();
            try
            {
                _deletedDocs.Add(docId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(long docId)
        {
            _lock.EnterReadLock();
            try
            {
                return _deletedDocs.Contains(docId);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void ExceptWith(IEnumerable<long> docs)
        {
            _lock.EnterWriteLock();
            try
            {
                _deletedDocs.ExceptWith(docs);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public HashSet<long> GetCopy()
        {
            _lock.EnterReadLock();
            try
            {
                return new HashSet<long>(_deletedDocs);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
