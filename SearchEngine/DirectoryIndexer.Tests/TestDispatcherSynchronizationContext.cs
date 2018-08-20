using System.Threading;
using DirectoryIndexerApp;

namespace DirectoryIndexer.Tests
{
    class TestDispatcherSynchronizationContext : SynchronizationContext
    {
        private readonly IDispatcher _dispatcher;

        public TestDispatcherSynchronizationContext(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _dispatcher.BeginInvoke(() => d.Invoke(state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            Post(d, state);
        }

        public override SynchronizationContext CreateCopy()
        {
            return new TestDispatcherSynchronizationContext(_dispatcher);
        }
    }
}