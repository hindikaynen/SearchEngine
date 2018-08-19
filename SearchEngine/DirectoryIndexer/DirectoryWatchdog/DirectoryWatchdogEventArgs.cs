namespace DirectoryIndexer
{
    class DirectoryWatchdogEventArgs
    {
        public DirectoryWatchdogEventArgs(string filePath, ChangeKind changeKind)
        {
            FilePath = filePath;
            ChangeKind = changeKind;
        }

        public string FilePath { get; set; }
        public ChangeKind ChangeKind { get; }
    }
}