using System;
using System.Threading;

namespace SearchEngine.MemoryStore
{
    class CleanUpTimer : IDisposable
    {
        private readonly Action _cleanUp;
        private readonly Timer _timer;

        public CleanUpTimer(Action cleanUp, int period)
        {
            _cleanUp = cleanUp;
            _timer = new Timer(OnTick, null, period, period);
        }

        private void OnTick(object state)
        {
            _cleanUp();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
