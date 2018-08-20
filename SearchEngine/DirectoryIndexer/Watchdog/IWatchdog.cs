using System;

namespace DirectoryIndexer
{
    interface IWatchdog : IDisposable
    {
        event EventHandler<WatchdogEventArgs> Changed;
        void Start();
    }
}