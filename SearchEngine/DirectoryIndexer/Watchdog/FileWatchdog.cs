using System;
using System.IO;

namespace DirectoryIndexer
{
    class FileWatchdog : IWatchdog
    {
        internal static readonly TimeSpan DuplicateChangedEventsTimeSpan = TimeSpan.FromMilliseconds(50);

        private readonly string _filePath;
        private readonly IWatchdogThread _watchdogThread;
        private readonly FileSystemWatcher _watcher;
        private DateTime _lastChanged;

        public FileWatchdog(string filePath, IWatchdogThread watchdogThread)
        {
            _filePath = filePath;
            _watchdogThread = watchdogThread;
            _watcher = new FileSystemWatcher(filePath);
            _watcher.Changed += OnChanged;
            _watcher.Deleted += OnDeleted;
        }

        public event EventHandler<WatchdogEventArgs> Changed;

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
            if (File.Exists(_filePath))
            {
                _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(_filePath, ChangeKind.Existed)));
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var now = DateTime.UtcNow;
            if (now - _lastChanged < DuplicateChangedEventsTimeSpan)
                return;
            _lastChanged = now;
            _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(_filePath, ChangeKind.Updated)));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            _watchdogThread.Enqueue(() => Changed?.Invoke(this, new WatchdogEventArgs(_filePath, ChangeKind.Deleted)));
        }
        
        public void Dispose()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }
}
