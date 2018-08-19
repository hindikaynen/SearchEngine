﻿using System;
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
        private const string TxtFilter = "*.txt";
        private readonly IDialogService _dialogService;
        private readonly IDispatcher _dispatcher;
        private readonly Indexer _indexer;
        private readonly DelegateCommand _addDirectoryCommand;
        private readonly DelegateCommand<string> _searchCommand;
        private readonly ObservableCollection<string> _trackingDirectories = new ObservableCollection<string>();
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
            _indexer = new Indexer(new SearchIndex(analyzer, store), TxtFilter);
            _indexer.IndexingProgress += OnIndexingProgress;

            _addDirectoryCommand = new DelegateCommand(AddDirectory);
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

        public bool IsSearching
        {
            get { return _isSearching; }
            set
            {
                if(_isSearching == value)
                    return;

                _isSearching = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> TrackingDirectories => _trackingDirectories;
        public ICollectionView SearchResults => _searchResultsSource.View;

        public ICommand AddDirectoryCommand => _addDirectoryCommand;
        public ICommand SearchCommand => _searchCommand;

        private void AddDirectory()
        {
            string directory;
            if(!_dialogService.ShowOpenFolderDialog(Resources.OpenDirectoryDescription, out directory))
                return;

            if(_trackingDirectories.Contains(directory))
                return;

            _indexer.AddDirectory(directory);
            _trackingDirectories.Add(directory);
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
