using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;
using SearchEngine;
using SearchEngine.Analysis;
using SearchEngine.MemoryStore;

namespace DirectoryIndexer.Tests
{
    [TestFixture]
    public class IndexerTests
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
        public void ShouldSearchExisting()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            var file2 = Path.Combine(_directory, "2.txt");
            var file3 = Path.Combine(_directory, "3.txt");
            var file4 = Path.Combine(_directory, "4.pdf");
            File.WriteAllText(file1, "hello world");
            File.WriteAllText(file2, "hello pretty world");
            File.WriteAllText(file3, "just hello");
            File.WriteAllText(file4, "hello ugly world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory, "*.txt");

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] {file1, file2}, result);

            indexer.Dispose();
        }

        [Test]
        public void ShouldSearchNew()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            var file2 = Path.Combine(_directory, "2.txt");
            var file3 = Path.Combine(_directory, "3.txt");
            var file4 = Path.Combine(_directory, "4.pdf");
            File.WriteAllText(file1, "hello world");
            File.WriteAllText(file2, "hello pretty world");
            File.WriteAllText(file3, "just hello");
            File.WriteAllText(file4, "hello ugly world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory, "*.txt");

            WaitForIndexed(indexer, events);

            var file5 = Path.Combine(_directory, "5.txt");
            File.WriteAllText(file5, "hello world again");

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file1, file2, file5 }, result);

            indexer.Dispose();
        }

        [Test]
        public void ShouldNotSearchDeleted()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            var file2 = Path.Combine(_directory, "2.txt");
            var file3 = Path.Combine(_directory, "3.txt");
            var file4 = Path.Combine(_directory, "4.pdf");
            File.WriteAllText(file1, "hello world");
            File.WriteAllText(file2, "hello pretty world");
            File.WriteAllText(file3, "just hello");
            File.WriteAllText(file4, "hello ugly world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory, "*.txt");

            WaitForIndexed(indexer, events);

            File.Delete(file1);

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file2 }, result);

            indexer.Dispose();
        }

        [Test]
        public void ShouldSearchRenamed()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            var file2 = Path.Combine(_directory, "2.txt");
            var file3 = Path.Combine(_directory, "3.txt");
            var file4 = Path.Combine(_directory, "4.pdf");
            File.WriteAllText(file1, "hello world");
            File.WriteAllText(file2, "hello pretty world");
            File.WriteAllText(file3, "just hello");
            File.WriteAllText(file4, "hello ugly world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory, "*.txt");

            WaitForIndexed(indexer, events);

            var file5 = Path.Combine(_directory, "5.txt");
            File.Move(file1, file5);

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file2, file5 }, result);

            indexer.Dispose();
        }

        [Test]
        public void ShouldSearchChanged()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            var file2 = Path.Combine(_directory, "2.txt");
            var file3 = Path.Combine(_directory, "3.txt");
            var file4 = Path.Combine(_directory, "4.pdf");
            File.WriteAllText(file1, "hello world");
            File.WriteAllText(file2, "hello pretty world");
            File.WriteAllText(file3, "just hello");
            File.WriteAllText(file4, "hello ugly world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory, "*.txt");

            WaitForIndexed(indexer, events);

            File.WriteAllText(file3, "hello world again");
            File.WriteAllText(file1, "smth another");

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file2, file3 }, result);

            indexer.Dispose();
        }

        [Test]
        public void ShouldAddFile()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            File.WriteAllText(file1, "hello world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddFile(file1);

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file1 }, result);

            File.WriteAllText(file1, "another content");

            WaitForIndexed(indexer, events);

            result = indexer.Search("hello world");
            CollectionAssert.IsEmpty(result);

            result = indexer.Search("another");
            CollectionAssert.AreEquivalent(new[] { file1 }, result);

            File.Delete(file1);

            WaitForIndexed(indexer, events);

            result = indexer.Search("another");
            CollectionAssert.IsEmpty(result);

            indexer.Dispose();
        }

        [Test]
        public void ShouldRemoveFileWhenRenamed()
        {
            var file1 = Path.Combine(_directory, "1.txt");
            File.WriteAllText(file1, "hello world");
            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();

            BlockingCollection<IndexingEventArgs> events = new BlockingCollection<IndexingEventArgs>();

            var indexer = new Indexer(new SearchIndex(analyzer, store));
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddFile(file1);

            WaitForIndexed(indexer, events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file1 }, result);

            File.Move(file1, Path.Combine(_directory, "2.txt"));

            WaitForIndexed(indexer, events);

            result = indexer.Search("hello");
            CollectionAssert.IsEmpty(result);

            indexer.Dispose();
        }

        private static void WaitForIndexed(Indexer indexer, BlockingCollection<IndexingEventArgs> events)
        {
            WaitForIndexingCompletedEvent(events);
            WaitForNoIndexingTasks(indexer);
        }
        
        private static void WaitForIndexingCompletedEvent(BlockingCollection<IndexingEventArgs> events)
        {
            bool isIndexing = false;
            IndexingEventArgs args;
            while (events.TryTake(out args, 50))
            {
                isIndexing = args.IsIndexing;
            }
            Assert.IsFalse(isIndexing);
        }

        private static void WaitForNoIndexingTasks(Indexer indexer, int timeout = 1000)
        {
            var sw = Stopwatch.StartNew();
            while (indexer.HasIndexingTasks)
            {
                if(sw.ElapsedMilliseconds > timeout)
                    throw new TimeoutException();
                Thread.Sleep(10);
            }
        }
    }
}
