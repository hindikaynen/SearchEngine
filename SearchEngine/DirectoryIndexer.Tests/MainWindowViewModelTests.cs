using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DirectoryIndexerApp;
using Moq;
using NUnit.Framework;

namespace DirectoryIndexer.Tests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_directory, true);
        }

        [Test]
        public void ShouldIndexAndSearch()
        {
            var filePath = Path.Combine(_directory, "1.txt");
            File.WriteAllText(filePath, "hello world");

            var dialogService = new Mock<IDialogService>();
            dialogService.Setup(x => x.ShowOpenFolderDialog(It.IsAny<string>(), out _directory)).Returns(true);

            var callbacks = new BlockingCollection<Action>();

            var dispatcher = new Mock<IDispatcher>();
            dispatcher.Setup(x => x.BeginInvoke(It.IsAny<Action>())).Callback((Action action) => callbacks.Add(action));

            SynchronizationContext.SetSynchronizationContext(new TestDispatcherSynchronizationContext(dispatcher.Object));

            var events = new BlockingCollection<PropertyChangedEventArgs>();

            var viewModel = new MainWindowViewModel(dialogService.Object, dispatcher.Object);
            viewModel.PropertyChanged += (o, e) => events.Add(e);
            viewModel.AddDirectoryCommand.Execute(_directory);

            WaitForIndexed(events, callbacks, viewModel);

            viewModel.SearchCommand.Execute("he?lo");

            WaitForSearchResults(viewModel, callbacks, new[] {filePath});
        }

        private static void WaitForIndexed(BlockingCollection<PropertyChangedEventArgs> events, BlockingCollection<Action> callbacks, MainWindowViewModel viewModel)
        {
            while (true)
            {
                ProcessCallbacks(callbacks);
                PropertyChangedEventArgs args;
                Assert.IsTrue(events.TryTake(out args, 5000));
                if (args.PropertyName == Properties.GetName<MainWindowViewModel>(vm => vm.IsIndexing))
                {
                    if (!viewModel.IsIndexing)
                        return;
                }
                Thread.Sleep(10);
            }
        }

        private void WaitForSearchResults(MainWindowViewModel viewModel, BlockingCollection<Action> callbacks, string[] searchResults, int timeout = 1000)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if(sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException("WaitForSearchResults");

                ProcessCallbacks(callbacks);

                var actual = viewModel.SearchResults.OfType<string>().ToArray();
                if(actual.OrderBy(x => x).SequenceEqual(searchResults.OrderBy(x => x)))
                    return;
            }
        }

        private static void ProcessCallbacks(BlockingCollection<Action> callbacks)
        {
            Action action;
            while (callbacks.TryTake(out action))
            {
                action();
            }
        }
    }
}
