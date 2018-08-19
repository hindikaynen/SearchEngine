namespace DirectoryIndexerApp
{
    public interface IDialogService
    {
        bool ShowOpenFolderDialog(string description, out string folder);
    }
}
