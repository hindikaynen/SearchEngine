using System;
using System.Collections.Concurrent;
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

            var indexer = new Indexer(new SearchIndex(analyzer, store), "*.txt");
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory);

            WaitForIndexed(events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] {file1, file2}, result);
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

            var indexer = new Indexer(new SearchIndex(analyzer, store), "*.txt");
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory);

            WaitForIndexed(events);

            var file5 = Path.Combine(_directory, "5.txt");
            File.WriteAllText(file5, "hello world again");

            WaitForIndexed(events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file1, file2, file5 }, result);
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

            var indexer = new Indexer(new SearchIndex(analyzer, store), "*.txt");
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory);

            WaitForIndexed(events);

            File.Delete(file1);

            WaitForIndexed(events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file2 }, result);
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

            var indexer = new Indexer(new SearchIndex(analyzer, store), "*.txt");
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory);

            WaitForIndexed(events);

            var file5 = Path.Combine(_directory, "5.txt");
            File.Move(file1, file5);

            WaitForIndexed(events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file2, file5 }, result);
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

            var indexer = new Indexer(new SearchIndex(analyzer, store), "*.txt");
            indexer.IndexingProgress += (o, e) => events.Add(e);
            indexer.AddDirectory(_directory);

            WaitForIndexed(events);

            File.WriteAllText(file3, "hello world again");
            File.WriteAllText(file1, "smth another");

            WaitForIndexed(events);

            var result = indexer.Search("hello world");
            CollectionAssert.AreEquivalent(new[] { file2, file3 }, result);
        }

        private static void WaitForIndexed(BlockingCollection<IndexingEventArgs> events)
        {
            Thread.Sleep(50);
            while (true)
            {
                IndexingEventArgs args;
                Assert.IsTrue(events.TryTake(out args, 5000));
                if(!args.IsIndexing)
                    return;
            }
        }
    }
}
