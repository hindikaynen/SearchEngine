using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using DirectoryIndexer;
using DirectoryIndexerApp.Properties;
using SearchEngine;
using SearchEngine.Analysis;
using SearchEngine.MemoryStore;

namespace DirectoryIndexerApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string TxtFilter = "*.cs";
        private readonly IDialogService _dialogService;
        private readonly IDispatcher _dispatcher;
        private readonly Indexer _indexer;
        private readonly DelegateCommand _addDirectoryCommand;
        private readonly DelegateCommand _addFilesCommand;
        private readonly DelegateCommand<string> _searchCommand;
        private readonly ObservableCollection<string> _trackingPaths = new ObservableCollection<string>();
        private readonly ObservableCollection<string> _searchResults = new ObservableCollection<string>();
        private readonly CollectionViewSource _searchResultsSource = new CollectionViewSource();
        private bool _isIndexing;
        private bool _isSearching;

        public MainWindowViewModel(IDialogService dialogService, IDispatcher dispatcher)
        {
            if (dialogService == null)
                throw new ArgumentNullException(nameof(dialogService));
            if (dispatcher == null)
                throw new ArgumentNullException(nameof(dispatcher));

            _dialogService = dialogService;
            _dispatcher = dispatcher;

            var analyzer = new SimpleAnalyzer();
            var store = new InMemoryStore();
            _indexer = new Indexer(new SearchIndex(analyzer, store));
            _indexer.IndexingProgress += OnIndexingProgress;

            _addDirectoryCommand = new DelegateCommand(AddDirectory);
            _addFilesCommand = new DelegateCommand(AddFiles);
            _searchCommand = new DelegateCommand<string>(Search);

            _searchResultsSource.Source = _searchResults;
            _searchResultsSource.SortDescriptions.Add(new SortDescription(string.Empty, ListSortDirection.Ascending));
        }

        public bool IsIndexing
        {
            get { return _isIndexing; }
            set
            {
                if(_isIndexing == value)
                    return;

                _isIndexing = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> TrackingPaths => _trackingPaths;
        public ICollectionView SearchResults => _searchResultsSource.View;

        public ICommand AddDirectoryCommand => _addDirectoryCommand;
        public ICommand AddFilesCommand => _addFilesCommand;
        public ICommand SearchCommand => _searchCommand;

        private void AddDirectory()
        {
            string directory;
            if(!_dialogService.ShowOpenFolderDialog(Resources.OpenDirectoryDescription, out directory))
                return;

            if(_trackingPaths.Contains(directory))
                return;

            _indexer.AddDirectory(directory, TxtFilter);
            _trackingPaths.Add(directory);
        }

        private void AddFiles()
        {
            IEnumerable<string> filePaths;
            if (!_dialogService.ShowOpenFilesDialog(Resources.OpenFilesDescription, out filePaths))
                return;

            foreach (var filePath in filePaths)
            {
                if (_trackingPaths.Contains(filePath))
                    return;

                _indexer.AddFile(filePath);
                _trackingPaths.Add(filePath);
            }
        }

        private void OnIndexingProgress(object sender, IndexingEventArgs e)
        {
            _dispatcher.BeginInvoke(() => IsIndexing = e.IsIndexing);
        }

        private async void Search(string searchString)
        {
            _searchResults.Clear();
            if(string.IsNullOrEmpty(searchString))
                return;

            var found = await SearchAsync(searchString);
            foreach (var result in found)
            {
                _searchResults.Add(result);
            }
        }

        private Task<IEnumerable<string>> SearchAsync(string searchString)
        {
            return Task.Factory.StartNew(() => _indexer.Search(searchString));
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
