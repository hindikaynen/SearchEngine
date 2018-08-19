﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SearchEngine;

namespace DirectoryIndexer
{
    public class DirectoryIndexer : IDisposable
    {
        const string NameField = "name";
        const string ContentField = "content";

        private readonly ISearchIndex _searchIndex;
        private readonly string _filter;
        private readonly ConcurrentBag<DirectoryWatchdog> _watchdogs = new ConcurrentBag<DirectoryWatchdog>();
        private readonly ConcurrentDictionary<string, Task> _taskQueue = new ConcurrentDictionary<string, Task>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _indexingCount;

        public DirectoryIndexer(ISearchIndex searchIndex, string filter)
        {
            if (searchIndex == null)
                throw new ArgumentNullException(nameof(searchIndex));

            _searchIndex = searchIndex;
            _filter = filter;
        }

        public event EventHandler<IndexingProgressEventArgs> IndexingProgress; 

        public void AddDirectory(string directoryPath)
        {
            var watchdog = new DirectoryWatchdog(directoryPath, _filter);
            watchdog.Changed += OnChanged;
            watchdog.Start();
            _watchdogs.Add(watchdog);
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

        private void OnChanged(object sender, DirectoryWatchdogEventArgs e)
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

        private void EnqueueTask(string filePath, Action action)
        {
            _taskQueue.AddOrUpdate(filePath,
                path => Task.Factory.StartNew(action, _cts.Token),
                (path, task) => task.ContinueWith(t => action(), _cts.Token));
        }

        private void Index(string filePath)
        {
            NotifyProgress(Interlocked.Increment(ref _indexingCount));
            try
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
            catch (OperationCanceledException)
            {
            }
            finally
            {
                NotifyProgress(Interlocked.Decrement(ref _indexingCount));
            }
        }

        private void Unindex(string filePath)
        {
            NotifyProgress(Interlocked.Increment(ref _indexingCount));
            try
            {
                _searchIndex.RemoveDocument(new Term(NameField, filePath));
            }
            finally
            {
                NotifyProgress(Interlocked.Decrement(ref _indexingCount));
            }
            
        }

        private void NotifyProgress(int count)
        {
            IndexingProgress?.Invoke(this, new IndexingProgressEventArgs(count));
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
            DirectoryWatchdog watchdog;
            while (_watchdogs.TryTake(out watchdog))
            {
                watchdog.Dispose();
            }
            _searchIndex.Dispose();
        }
    }
}
