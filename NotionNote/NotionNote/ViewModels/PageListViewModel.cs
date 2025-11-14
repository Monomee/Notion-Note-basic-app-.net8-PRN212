using NotionNote.Commands;
using NotionNote.Models;
using NotionNote.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NotionNote.ViewModels
{
    public class PageItemViewModel : INotifyPropertyChanged
    {
        private readonly Page _page;
        private readonly IPageService _pageService;
        private bool _isEditing;
        private string _title;


        public PageItemViewModel(Page page, IPageService pageService)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _title = page.Title;
        }

        public Page Page => _page;

        public int PageId => _page.PageId;
        
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string? Content => _page.Content;
        public DateTime? CreatedAt => _page.CreatedAt;
        public DateTime? UpdatedAt => _page.UpdatedAt;
        public bool? IsPinned => _page.IsPinned;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                    
                    if (!value && _page.Title != _title)
                    {
                        _page.Title = _title;
                        _page.UpdatedAt = DateTime.Now;
                        _pageService.UpdatePage(_page);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PageListViewModel : INotifyPropertyChanged
    {
        private readonly IPageService _pageService;
        private ObservableCollection<PageItemViewModel> _pages = new();
        private ObservableCollection<PageItemViewModel> _filteredPages = new();
        private string _searchText = string.Empty;
        private PageItemViewModel? _selected;
        private int? _currentWorkspaceId;

        public PageListViewModel(IPageService pageService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            
            AddPageCommand = new RelayCommand(AddPage, CanAddPage);
            DeletePageCommand = new RelayCommand(DeletePage, CanDeletePage);
            RefreshCommand = new RelayCommand(RefreshPages);
            
            _filteredPages = new ObservableCollection<PageItemViewModel>(_pages);
        }

        #region Properties

        public ObservableCollection<PageItemViewModel> Pages
        {
            get => _pages;
            private set
            {
                if (_pages != value)
                {
                    _pages = value;
                    OnPropertyChanged();
                    UpdateFilteredPages();
                }
            }
        }

        public ObservableCollection<PageItemViewModel> FilteredPages
        {
            get => _filteredPages;
            private set
            {
                if (_filteredPages != value)
                {
                    _filteredPages = value;
                    OnPropertyChanged();
                    UpdateIsEmpty();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    UpdateFilteredPages();
                }
            }
        }

        public PageItemViewModel? Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? CurrentWorkspaceId
        {
            get => _currentWorkspaceId;
            set
            {
                if (_currentWorkspaceId != value)
                {
                    _currentWorkspaceId = value;
                    OnPropertyChanged();
                    Selected = null;
                    RefreshPages();
                }
            }
        }

        public bool IsEmpty
        {
            get => _filteredPages.Count == 0;
        }

        #endregion

        #region Commands

        public ICommand AddPageCommand { get; }
        public ICommand DeletePageCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Command Implementations

        private void AddPage()
        {
            if (CurrentWorkspaceId == null) return;

            var newPage = new Page
            {
                Title = "Untitled Page",
                Content = string.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsPinned = false,
                WorkspaceId = CurrentWorkspaceId.Value
            };

            var createdPage = _pageService.CreatePage(newPage);
            var pageItem = new PageItemViewModel(createdPage, _pageService);
            
            _pages.Add(pageItem);
            UpdateFilteredPages();
            
            Selected = pageItem;
            pageItem.IsEditing = true;
        }

        private bool CanAddPage()
        {
            return CurrentWorkspaceId != null;
        }

        private void DeletePage()
        {
            if (Selected == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{Selected.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _pageService.DeletePage(Selected.PageId);
            _pages.Remove(Selected);
            UpdateFilteredPages();
            Selected = null;
        }

        private bool CanDeletePage()
        {
            return Selected != null;
        }

        private void RefreshPages()
        {
            if (CurrentWorkspaceId == null) return;

            var pages = _pageService.GetPagesByWorkspaceId(CurrentWorkspaceId.Value);
            _pages.Clear();
            
            foreach (var page in pages)
            {
                _pages.Add(new PageItemViewModel(page, _pageService));
            }
            
            UpdateFilteredPages();
        }


        #endregion

        #region Helper Methods

        internal void UpdateFilteredPages()
        {
            var filtered = _pages.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(p => 
                    p.Title.ToLower().Contains(searchLower) ||
                    (p.Content != null && p.Content.ToLower().Contains(searchLower)));
            }

            filtered = filtered
                .OrderByDescending(p => p.IsPinned ?? false)
                .ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt ?? DateTime.MinValue);

            _filteredPages.Clear();
            foreach (var item in filtered)
            {
                _filteredPages.Add(item);
            }

            UpdateIsEmpty();
        }

        private void UpdateIsEmpty()
        {
            OnPropertyChanged(nameof(IsEmpty));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
