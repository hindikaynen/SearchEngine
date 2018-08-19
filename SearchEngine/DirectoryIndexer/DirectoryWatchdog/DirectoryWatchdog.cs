using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace DirectoryIndexer
{
    class DirectoryWatchdog : IDisposable
    {
        internal static readonly TimeSpan DuplicateChangedEventsTimeSpan = TimeSpan.FromMilliseconds(50);

        private readonly string _directoryPath;
        private readonly string _filter;
        private readonly Regex _filterRegex;
        private readonly FileSystemWatcher _watcher;
        private readonly BlockingCollection<DirectoryWatchdogEventArgs> _events = new BlockingCollection<DirectoryWatchdogEventArgs>();
        private readonly ConcurrentDictionary<string, DateTime> _lastUpdated = new ConcurrentDictionary<string, DateTime>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Thread _processEventsThread;
        
        public DirectoryWatchdog(string directoryPath, string filter)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            _directoryPath = directoryPath;
            _filter = filter;
            _filterRegex = GetFilterRegex(filter);
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));
            if (!Directory.Exists(directoryPath))
                throw new InvalidOperationException($"Directory {directoryPath} does not exist");

            _watcher = new FileSystemWatcher(directoryPath, filter)
            {
                IncludeSubdirectories = true
            };
            _watcher.Created += OnCreated;
            _watcher.Renamed += OnRenamed;
            _watcher.Changed += OnChanged;
            _watcher.Deleted += OnDeleted;
        }

        public event EventHandler<DirectoryWatchdogEventArgs> Changed;

        public void Start()
        {
            if(_processEventsThread != null)
                throw new InvalidOperationException("Already started");

            NofifyExisted();
            _watcher.EnableRaisingEvents = true;
            _processEventsThread = new Thread(ProcessEvents)
            {
                IsBackground = true
            };
            _processEventsThread.Start();
        }

        private void ProcessEvents()
        {
            foreach (var args in _events.GetConsumingEnumerable(_cts.Token))
            {
                Changed?.Invoke(this, args);
            }
        }

        private void NofifyExisted()
        {
            foreach (var filePath in Directory.GetFiles(_directoryPath, _filter, SearchOption.AllDirectories))
            {
                Changed?.Invoke(this, new DirectoryWatchdogEventArgs(filePath, ChangeKind.Existed));
            }            
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            _lastUpdated[e.FullPath] = DateTime.UtcNow;
            _events.Add(new DirectoryWatchdogEventArgs(e.FullPath, ChangeKind.Created));
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var now = DateTime.UtcNow;
            bool filtered = false;
            _lastUpdated.AddOrUpdate(e.FullPath, now, (key, oldValue) =>
            {
                if (now - oldValue < DuplicateChangedEventsTimeSpan)
                    filtered = true;
                return now;
            });

            if (!filtered)
            {
                _events.Add(new DirectoryWatchdogEventArgs(e.FullPath, ChangeKind.Updated));
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            _events.Add(new DirectoryWatchdogEventArgs(e.FullPath, ChangeKind.Deleted));
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            _events.Add(new DirectoryWatchdogEventArgs(e.OldFullPath, ChangeKind.Deleted));
            if(MatchesFilter(e.FullPath))
                _events.Add(new DirectoryWatchdogEventArgs(e.FullPath, ChangeKind.Created));
        }

        private static Regex GetFilterRegex(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return null;

            return new Regex(
                '^' +
                filter
                    .Replace(".", "[.]")
                    .Replace("*", ".*")
                    .Replace("?", ".")
                + '$',
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        private bool MatchesFilter(string filePath)
        {
            if (_filterRegex == null)
                return true;

            var fileName = Path.GetFileName(filePath);
            return fileName != null && _filterRegex.IsMatch(fileName);
        }

        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _cts.Cancel();
            _processEventsThread?.Join();
        }
    }
}
