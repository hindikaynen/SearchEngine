using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using SearchEngine;

namespace DirectoryIndexer
{
    public class Indexer : IDisposable
    {
        const string NameField = "name";
        const string ContentField = "content";

        private readonly ISearchIndex _searchIndex;
        private readonly ConcurrentBag<IWatchdog> _watchdogs = new ConcurrentBag<IWatchdog>();
        private readonly WatchdogThread _watchdogThread = new WatchdogThread();
        private readonly ConcurrentDictionary<string, Task> _taskQueue = new ConcurrentDictionary<string, Task>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly LimitedConcurrencyLevelTaskScheduler _scheduler;
        private int _indexingCount;
        
        public Indexer(ISearchIndex searchIndex)
        {
            if (searchIndex == null)
                throw new ArgumentNullException(nameof(searchIndex));

            _searchIndex = searchIndex;
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(Environment.ProcessorCount);
        }

        public event EventHandler<IndexingEventArgs> IndexingProgress;
        internal bool HasIndexingTasks => _taskQueue.Any();

        public void AddDirectory(string directoryPath, string filter)
        {
            AddDirectoryWatchdog(directoryPath, filter);
        }

        public void AddFile(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            var filter = Path.GetFileName(filePath);
            AddDirectoryWatchdog(directory, filter);
        }

        public IEnumerable<string> Search(string searchString)
        {
            var queryParser = new QueryParser(ContentField, DefaultOperator.And);
            var query = queryParser.Parse(searchString);
            var hits = _searchIndex.Search(query);
            foreach (var hit in hits)
            {
                var filePath = _searchIndex.GetFieldValue(hit, NameField);
                if(!string.IsNullOrEmpty(filePath))
                    yield return filePath;
            }
        }

        private void AddDirectoryWatchdog(string directoryPath, string filter)
        {
            var watchdog = new DirectoryWatchdog(directoryPath, filter, _watchdogThread);
            watchdog.Changed += OnChanged;
            _watchdogs.Add(watchdog);
            Notifying(() => Task.Factory.StartNew(watchdog.Start));
        }

        private void OnChanged(object sender, WatchdogEventArgs e)
        {
            switch (e.ChangeKind)
            {
                case ChangeKind.Created:
                case ChangeKind.Existed:
                case ChangeKind.Updated:
                    EnqueueTask(e.FilePath, () => Index(e.FilePath));
                    break;
                case ChangeKind.Deleted:
                    EnqueueTask(e.FilePath, () => Unindex(e.FilePath));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private async void Notifying(Func<Task> taskFactory)
        {
            int count = Interlocked.Increment(ref _indexingCount);
            NotifyProgress(count - 1, count);
            try
            {
                await taskFactory();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                count = Interlocked.Decrement(ref _indexingCount);
                NotifyProgress(count + 1, count);
            }
        }

        private void EnqueueTask(string filePath, Action action)
        {
            Notifying(() => EnqueueTaskCore(filePath, action));
        }

        private Task EnqueueTaskCore(string filePath, Action action)
        {
            var indexTask = _taskQueue.AddOrUpdate(filePath,
                path => Task.Factory.StartNew(action, _cts.Token, TaskCreationOptions.None, _scheduler),
                (path, task) => task.ContinueWith(t => action(), _cts.Token, TaskContinuationOptions.None, _scheduler));
            indexTask.ContinueWith(t => _taskQueue.RemoveByKeyValue(filePath, indexTask));
            return indexTask;
        }

        private void Index(string filePath)
        {
            Stream stream;
            if(!WaitForFileUnlocked(filePath, out stream))
                return;

            var document = new Document();
            document.AddField(new StringField(NameField, filePath, FieldFlags.Stored));
            document.AddField(new TextField(ContentField, stream, FieldFlags.Analyzed));

            _searchIndex.RemoveDocument(new Term(NameField, filePath));
            _searchIndex.AddDocument(document);
        }

        private void Unindex(string filePath)
        {
            _searchIndex.RemoveDocument(new Term(NameField, filePath));
        }

        private void NotifyProgress(int oldValue, int newValue)
        {
            bool oldIsIndexing = oldValue > 0;
            bool newIsIndexing = newValue > 0;
            if(oldIsIndexing == newIsIndexing)
                return;

            IndexingProgress?.Invoke(this, new IndexingEventArgs(newIsIndexing));
        }

        private bool WaitForFileUnlocked(string filePath, out Stream stream)
        {
            while (true)
            {
                _cts.Token.ThrowIfCancellationRequested();

                try
                {
                    stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    stream = null;
                    return false;
                }
                catch
                {
                    Thread.Sleep(10);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _watchdogThread.Dispose();
            IWatchdog watchdog;
            while (_watchdogs.TryTake(out watchdog))
            {
                watchdog.Dispose();
            }
            _searchIndex.Dispose();
        }
    }
}
