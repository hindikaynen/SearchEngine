using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirectoryIndexer
{
    class DirectoryWatchdog : IWatchdog
    {
        internal static readonly TimeSpan DuplicateChangedEventsTimeSpan = TimeSpan.FromMilliseconds(50);

        private readonly string _directoryPath;
        private readonly string _filter;
        private readonly IWatchdogThread _watchdogThread;
        private readonly Regex _filterRegex;
        private readonly FileSystemWatcher _watcher;
        private readonly ConcurrentDictionary<string, DateTime> _lastUpdated = new ConcurrentDictionary<string, DateTime>();
        
        public DirectoryWatchdog(string directoryPath, string filter, IWatchdogThread watchdogThread)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            _directoryPath = directoryPath;
            _filter = filter;
            _watchdogThread = watchdogThread;
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

        public event EventHandler<WatchdogEventArgs> Changed;

        public void Start()
        {
            Task.Factory.StartNew(NofifyExisted);
            _watcher.EnableRaisingEvents = true;
        }
        
        private void NofifyExisted()
        {
            try
            {
                foreach (var filePath in Directory.GetFiles(_directoryPath, _filter, SearchOption.AllDirectories))
                {
                    Changed?.Invoke(this, new WatchdogEventArgs(filePath, ChangeKind.Existed));
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            _lastUpdated[e.FullPath] = DateTime.UtcNow;
            _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(e.FullPath, ChangeKind.Created)));
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
                _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(e.FullPath, ChangeKind.Updated)));
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(e.FullPath, ChangeKind.Deleted)));
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(e.OldFullPath, ChangeKind.Deleted)));
            if(MatchesFilter(e.FullPath))
                _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(e.FullPath, ChangeKind.Created)));
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
        }
    }
}
