using System.Windows.Forms;

namespace DirectoryIndexerApp
{
    class DialogService : IDialogService
    {
        public bool ShowOpenFolderDialog(string description, out string folder)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = description
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                folder = dialog.SelectedPath;
                return true;
            }
            folder = null;
            return false;
        }
    }
}