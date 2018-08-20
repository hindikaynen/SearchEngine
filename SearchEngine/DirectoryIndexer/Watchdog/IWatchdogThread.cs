using System;

namespace DirectoryIndexer
{
    interface IWatchdogThread : IDisposable
    {
        void Enqueue(Action action);
    }
}
