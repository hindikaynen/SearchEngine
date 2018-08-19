using System;

namespace DirectoryIndexer
{
    public class IndexingEventArgs : EventArgs
    {
        public IndexingEventArgs(bool isIndexing)
        {
            IsIndexing = isIndexing;
        }

        public bool IsIndexing { get; }
    }
}
