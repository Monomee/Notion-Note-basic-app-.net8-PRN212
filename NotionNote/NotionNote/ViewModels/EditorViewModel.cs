using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NotionNote.Models;
using NotionNote.Services;
using NotionNote.Commands;

namespace NotionNote.ViewModels
{
    public class EditorViewModel : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _content = string.Empty;
        private bool _isEmpty = true;
        private bool _isDirty = false;
        private DateTime? _lastSavedAt;
        private Page? _currentPage;
        private bool _isBusy;
        private string _newTagInput = string.Empty;
        private ObservableCollection<TagItemViewModel> _selectedTags = new();
        private ObservableCollection<TagItemViewModel> _availableTags = new();

        private readonly IPageService _pageService;
        private readonly ITagService _tagService;
        private int? _currentWorkspaceId;
        
        public event EventHandler<Page>? PageUpdated;
        
        public EditorViewModel(IPageService pageService, ITagService tagService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            
            // Initialize commands
            PinCommand = new RelayCommand(PinPage, CanPinPage);
            DeleteCommand = new RelayCommand(DeletePage, CanDeletePage);
            SaveCommand = new RelayCommand(SavePage, CanSavePage);
            AddTagCommand = new RelayCommand(AddTag, CanAddTag);
            RemoveTagCommand = new RelayCommand<TagItemViewModel>(RemoveTag);
            
            LoadAvailableTags();
        }

        #region Properties

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                    SetDirty();
                    UpdateIsEmpty();
                }
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                    SetDirty();
                    UpdateIsEmpty();
                }
            }
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            private set
            {
                if (_isEmpty != value)
                {
                    _isEmpty = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? LastSavedAt
        {
            get => _lastSavedAt;
            private set
            {
                if (_lastSavedAt != value)
                {
                    _lastSavedAt = value;
                    OnPropertyChanged();
                }
            }
        }

        public Page? CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    LoadPageData();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TagItemViewModel> SelectedTags
        {
            get => _selectedTags;
            private set
            {
                if (_selectedTags != value)
                {
                    _selectedTags = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TagItemViewModel> AvailableTags
        {
            get => _availableTags;
            private set
            {
                if (_availableTags != value)
                {
                    _availableTags = value;
                    OnPropertyChanged();
                }
            }
        }

        public string NewTagInput
        {
            get => _newTagInput;
            set
            {
                if (_newTagInput != value)
                {
                    _newTagInput = value;
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
                }
            }
        }

        #endregion

        #region Commands

        public ICommand PinCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }

        #endregion

        #region Command Implementations

        private void PinPage()
        {
            if (CurrentPage == null) return;

            IsBusy = true;
            try
            {
                CurrentPage.IsPinned = !CurrentPage.IsPinned;
                CurrentPage.UpdatedAt = DateTime.Now;
                _pageService.UpdatePage(CurrentPage);

                // Notify UI
                OnPropertyChanged(nameof(CurrentPage));

                // THÊM: Fire event để PageListViewModel biết
                PageUpdated?.Invoke(this, CurrentPage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanPinPage()
        {
            return CurrentPage != null;
        }

        private void DeletePage()
        {
            if (CurrentPage == null) return;
            IsBusy = true;
            try
            {
                _pageService.DeletePage(CurrentPage.PageId);
                ClearPage();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanDeletePage()
        {
            return CurrentPage != null;
        }

        private void SavePage()
        {
            IsBusy = true;
            try
            {
                if (CurrentPage != null)
                {
                    CurrentPage.Title = Title;
                    CurrentPage.Content = Content;
                    CurrentPage.UpdatedAt = DateTime.Now;
                    
                    // Update tags
                    UpdatePageTags();
                    
                    _pageService.UpdatePage(CurrentPage);
                    LastSavedAt = DateTime.Now;
                    IsDirty = false;
                    
                    // Fire event to notify PageListViewModel to refresh
                    PageUpdated?.Invoke(this, CurrentPage);
                }
                else
                {
                    if (!CurrentWorkspaceId.HasValue)
                    {
                        // Cannot create page without workspace
                        return;
                    }

                    var newPage = new Page
                    {
                        Title = string.IsNullOrWhiteSpace(Title) ? "Untitled Page" : Title,
                        Content = Content,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsPinned = false,
                        WorkspaceId = CurrentWorkspaceId.Value
                    };
                    CurrentPage = _pageService.CreatePage(newPage);
                    
                    // Update tags for new page
                    UpdatePageTags();
                    _pageService.UpdatePage(CurrentPage);
                    
                    LastSavedAt = DateTime.Now;
                    IsDirty = false;
                    
                    // Fire event to notify PageListViewModel to refresh
                    PageUpdated?.Invoke(this, CurrentPage);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSavePage()
        {
            return !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Content);
        }

        #endregion

        #region Helper Methods

        private void LoadPageData()
        {
            if (CurrentPage != null)
            {
                Title = CurrentPage.Title ?? string.Empty;
                Content = CurrentPage.Content ?? string.Empty;
                LastSavedAt = CurrentPage.UpdatedAt ?? CurrentPage.CreatedAt;
                
                // Load tags
                LoadPageTags();
                
                IsDirty = false;
            }
            else
            {
                ClearPage();
            }
        }

        private void LoadPageTags()
        {
            SelectedTags.Clear();
            
            if (CurrentPage != null)
            {
                // Reload page with tags
                var pageWithTags = _pageService.GetPageById(CurrentPage.PageId);
                if (pageWithTags != null && pageWithTags.Tags != null)
                {
                    foreach (var tag in pageWithTags.Tags)
                    {
                        SelectedTags.Add(new TagItemViewModel(tag));
                    }
                }
            }
            
            // Refresh available tags
            LoadAvailableTags();
        }

        private void UpdatePageTags()
        {
            if (CurrentPage == null) return;

            // Reload page with tags to get current state
            var pageWithTags = _pageService.GetPageById(CurrentPage.PageId);
            if (pageWithTags == null) return;

            // Get current tag IDs
            var currentTagIds = pageWithTags.Tags?.Select(t => t.TagId).ToHashSet() ?? new HashSet<int>();
            
            // Get selected tag IDs
            var selectedTagIds = SelectedTags.Select(t => t.TagId).ToHashSet();

            // Remove tags that are no longer selected
            foreach (var tagId in currentTagIds)
            {
                if (!selectedTagIds.Contains(tagId))
                {
                    _tagService.RemoveTagFromPage(CurrentPage.PageId, tagId);
                }
            }

            // Add new tags
            foreach (var tagItem in SelectedTags)
            {
                if (!currentTagIds.Contains(tagItem.TagId))
                {
                    _tagService.AddTagToPage(CurrentPage.PageId, tagItem.TagId);
                }
            }

            // Reload page to get updated tags
            CurrentPage = _pageService.GetPageById(CurrentPage.PageId);
        }

        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTagInput))
                return;

            var tagName = NewTagInput.Trim();
            
            // Check if tag already selected
            if (SelectedTags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            {
                NewTagInput = string.Empty;
                return;
            }

            // Find or create tag
            var tag = _tagService.GetTagByName(tagName);
            if (tag == null)
            {
                tag = _tagService.CreateTag(tagName);
            }

            // Add to selected tags
            SelectedTags.Add(new TagItemViewModel(tag));
            NewTagInput = string.Empty;
            SetDirty();
            
            // Refresh available tags
            LoadAvailableTags();
        }

        private bool CanAddTag()
        {
            return !string.IsNullOrWhiteSpace(NewTagInput) && !IsBusy;
        }

        private void RemoveTag(TagItemViewModel? tagItem)
        {
            if (tagItem == null) return;
            
            SelectedTags.Remove(tagItem);
            SetDirty();
            
            // Refresh available tags
            LoadAvailableTags();
        }

        private void LoadAvailableTags()
        {
            AvailableTags.Clear();
            var allTags = _tagService.GetAllTags();
            
            foreach (var tag in allTags)
            {
                // Only add tags that are not already selected
                if (!SelectedTags.Any(st => st.TagId == tag.TagId))
                {
                    AvailableTags.Add(new TagItemViewModel(tag));
                }
            }
        }

        private void ClearPage()
        {
            Title = string.Empty;
            Content = string.Empty;
            CurrentPage = null;
            LastSavedAt = null;
            IsDirty = false;
            SelectedTags.Clear();
            NewTagInput = string.Empty;
        }

        internal void SetDirty()
        {
            IsDirty = true;
        }

        private void UpdateIsEmpty()
        {
            IsEmpty = string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Content);
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
