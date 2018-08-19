using System;

namespace DirectoryIndexer
{
    public class IndexingProgressEventArgs : EventArgs
    {
        public IndexingProgressEventArgs(int indexingCount)
        {
            IndexingCount = indexingCount;
        }

        public int IndexingCount { get; }
    }
}
