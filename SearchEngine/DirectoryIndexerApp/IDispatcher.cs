using System;
using System.Windows.Threading;

namespace DirectoryIndexerApp
{
    public interface IDispatcher
    {
        void BeginInvoke(Action action);
    }

    class DispatcherImpl : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherImpl(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void BeginInvoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }
    }
}