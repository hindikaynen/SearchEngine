using System.Collections.Generic;

namespace DirectoryIndexerApp
{
    public interface IDialogService
    {
        bool ShowOpenFolderDialog(string description, out string folder);
        bool ShowOpenFilesDialog(string description, out IEnumerable<string> filePaths);
    }
}
