using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DirectoryIndexer
{
    class WatchdogThread : IWatchdogThread
    {
        private readonly BlockingCollection<Action> _actions = new BlockingCollection<Action>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Thread _thread;

        public WatchdogThread()
        {
            _thread = new Thread(Processing) { IsBackground = true };
        }

        private void Processing()
        {
            foreach (var action in _actions.GetConsumingEnumerable(_cts.Token))
            {
                action();
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _actions.CompleteAdding();
            _thread.Join();
        }

        public void Enqueue(Action action)
        {
            if(!_actions.IsAddingCompleted)
                _actions.Add(action);
        }
    }
}