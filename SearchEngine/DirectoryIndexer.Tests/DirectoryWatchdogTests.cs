using System;
using System.Collections.Concurrent;
using System.IO;
using NUnit.Framework;

namespace DirectoryIndexer.Tests
{
    [TestFixture]
    public class DirectoryWatchdogTests
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
        public void ShouldFireOnExistingFiles()
        {
            File.WriteAllBytes(Path.Combine(_directory, "1.txt"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(_directory, "2.pdf"), new byte[] { 1, 2, 3 });
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(_directory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Existed, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        [Test]
        public void ShouldFireOnExistingFilesInSubdirectory()
        {
            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);
            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Existed, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        [Test]
        public void ShouldFireIfCreatedRemoved()
        {
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            File.WriteAllBytes(Path.Combine(_directory, "1.pdf"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            File.Delete(Path.Combine(subdirectory, "1.txt"));

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Deleted, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        [Test]
        public void ShouldFireIfRenamed()
        {
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            File.WriteAllBytes(Path.Combine(_directory, "1.pdf"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            File.Move(Path.Combine(subdirectory, "1.txt"), Path.Combine(subdirectory, "2.txt"));

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Deleted, ev.ChangeKind);

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "2.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        [Test]
        public void ShouldFireIfMoved()
        {
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            File.WriteAllBytes(Path.Combine(_directory, "1.pdf"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            File.Move(Path.Combine(subdirectory, "1.txt"), Path.Combine(_directory, "1.txt"));

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Deleted, ev.ChangeKind);

            ev = events.Take();

            Assert.AreEqual(Path.Combine(_directory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        [Test]
        public void ShouldFireIfRenamedToFiltered()
        {
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            File.WriteAllBytes(Path.Combine(_directory, "1.pdf"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            File.Move(Path.Combine(subdirectory, "1.txt"), Path.Combine(subdirectory, "2.pdf"));

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Deleted, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        [Test]
        public void ShouldFireIfMovedOutside()
        {
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            File.WriteAllBytes(Path.Combine(_directory, "1.pdf"), new byte[] { 1, 2, 3 });
            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            File.Move(Path.Combine(subdirectory, "1.txt"), tempFilePath);

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Deleted, ev.ChangeKind);
            
            NoMoreEvents(events);

            watchdog.Dispose();

            File.Delete(tempFilePath);
        }

        [Test]
        public void ShouldFireIfMovedFromOutside()
        {
            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            File.WriteAllBytes(tempFilePath, new byte[] { 1, 2, 3 });
            
            File.Move(tempFilePath, Path.Combine(subdirectory, "1.txt"));

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Created, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();

            File.Delete(tempFilePath);
        }

        [Test]
        public void ShouldFireIfChanged()
        {
            var subdirectory = Path.Combine(_directory, "sub");
            Directory.CreateDirectory(subdirectory);

            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 1, 2, 3 });

            var watchdog = new DirectoryWatchdog(_directory, "*.txt");
            var events = new BlockingCollection<DirectoryWatchdogEventArgs>();
            watchdog.Changed += (o, e) => events.Add(e);
            watchdog.Start();

            var ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Existed, ev.ChangeKind);

            File.WriteAllBytes(Path.Combine(subdirectory, "1.txt"), new byte[] { 3, 4, 5 });

            ev = events.Take();

            Assert.AreEqual(Path.Combine(subdirectory, "1.txt"), ev.FilePath);
            Assert.AreEqual(ChangeKind.Updated, ev.ChangeKind);

            NoMoreEvents(events);

            watchdog.Dispose();
        }

        private static void NoMoreEvents(BlockingCollection<DirectoryWatchdogEventArgs> events)
        {
            DirectoryWatchdogEventArgs args;
            if(events.TryTake(out args, 100))
                Assert.Fail($"No more events expected but was {args.ChangeKind} on {args.FilePath}");
        }
    }
}
