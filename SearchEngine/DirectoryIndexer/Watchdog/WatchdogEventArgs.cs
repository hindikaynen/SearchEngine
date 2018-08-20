namespace DirectoryIndexer
{
    class WatchdogEventArgs
    {
        public WatchdogEventArgs(string filePath, ChangeKind changeKind)
        {
            FilePath = filePath;
            ChangeKind = changeKind;
        }

        public string FilePath { get; set; }
        public ChangeKind ChangeKind { get; }
    }
}