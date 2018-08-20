using System.Collections.Generic;
using System.Linq;
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

        public bool ShowOpenFilesDialog(string description, out IEnumerable<string> filePaths)
        {
            var dialog = new OpenFileDialog
            {
                Title = description,
                Multiselect = true
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePaths = dialog.FileNames.ToList();
                return true;
            }
            filePaths = null;
            return false;
        }
    }
}