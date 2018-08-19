using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

            var dispatcher = new Mock<IDispatcher>();
            dispatcher.Setup(x => x.BeginInvoke(It.IsAny<Action>())).Callback((Action action) => action());

            var events = new BlockingCollection<PropertyChangedEventArgs>();

            var viewModel = new MainWindowViewModel(dialogService.Object, dispatcher.Object);
            viewModel.PropertyChanged += (o, e) => events.Add(e);
            viewModel.AddDirectoryCommand.Execute(_directory);

            WaitForIndexed(events, viewModel);

            viewModel.SearchCommand.Execute("he?lo");

            WaitForSearchResults(viewModel, new[] {filePath});
        }

        private static void WaitForIndexed(BlockingCollection<PropertyChangedEventArgs> events, MainWindowViewModel viewModel)
        {
            while (true)
            {
                PropertyChangedEventArgs args;
                Assert.IsTrue(events.TryTake(out args, 5000));
                if (args.PropertyName == Properties.GetName<MainWindowViewModel>(vm => vm.IsIndexing))
                {
                    if (!viewModel.IsIndexing)
                        return;
                }
            }
        }

        private void WaitForSearchResults(MainWindowViewModel viewModel, string[] searchResults, int timeout = 1000)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                if(sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException("WaitForSearchResults");

                try
                {
                    var actual = viewModel.SearchResults.ToArray();
                    if(actual.OrderBy(x => x).SequenceEqual(searchResults.OrderBy(x => x)))
                        return;
                }
                catch
                {
                    //
                }
            }
        }
    }
}
