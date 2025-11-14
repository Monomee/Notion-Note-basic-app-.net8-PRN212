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

            CurrentPage.IsPinned = !CurrentPage.IsPinned;
            CurrentPage.UpdatedAt = DateTime.Now;
            _pageService.UpdatePage(CurrentPage);

            OnPropertyChanged(nameof(CurrentPage));

            PageUpdated?.Invoke(this, CurrentPage);
        }

        private bool CanPinPage()
        {
            return CurrentPage != null;
        }

        private void DeletePage()
        {
            if (CurrentPage == null) return;
            _pageService.DeletePage(CurrentPage.PageId);
            ClearPage();
        }

        private bool CanDeletePage()
        {
            return CurrentPage != null;
        }

        private void SavePage()
        {
            if (CurrentPage != null)
            {
                CurrentPage.Title = Title;
                CurrentPage.Content = Content;
                CurrentPage.UpdatedAt = DateTime.Now;
                
                UpdatePageTags();
                
                _pageService.UpdatePage(CurrentPage);
                LastSavedAt = DateTime.Now;
                IsDirty = false;
                
                PageUpdated?.Invoke(this, CurrentPage);
            }
            else
            {
                if (!CurrentWorkspaceId.HasValue)
                {
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
                
                UpdatePageTags();
                _pageService.UpdatePage(CurrentPage);
                
                LastSavedAt = DateTime.Now;
                IsDirty = false;
                
                PageUpdated?.Invoke(this, CurrentPage);
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
                var pageWithTags = _pageService.GetPageById(CurrentPage.PageId);
                if (pageWithTags != null && pageWithTags.Tags != null)
                {
                    foreach (var tag in pageWithTags.Tags)
                    {
                        SelectedTags.Add(new TagItemViewModel(tag));
                    }
                }
            }
            
            LoadAvailableTags();
        }

        private void UpdatePageTags()
        {
            if (CurrentPage == null) return;

            var pageWithTags = _pageService.GetPageById(CurrentPage.PageId);
            if (pageWithTags == null) return;

            var currentTagIds = pageWithTags.Tags?.Select(t => t.TagId).ToHashSet() ?? new HashSet<int>();
            
            var selectedTagIds = SelectedTags.Select(t => t.TagId).ToHashSet();

            foreach (var tagId in currentTagIds)
            {
                if (!selectedTagIds.Contains(tagId))
                {
                    _tagService.RemoveTagFromPage(CurrentPage.PageId, tagId);
                }
            }

            foreach (var tagItem in SelectedTags)
            {
                if (!currentTagIds.Contains(tagItem.TagId))
                {
                    _tagService.AddTagToPage(CurrentPage.PageId, tagItem.TagId);
                }
            }

            CurrentPage = _pageService.GetPageById(CurrentPage.PageId);
        }

        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTagInput))
                return;

            var tagName = NewTagInput.Trim();
            
            if (SelectedTags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            {
                NewTagInput = string.Empty;
                return;
            }

            var tag = _tagService.GetTagByName(tagName);
            if (tag == null)
            {
                tag = _tagService.CreateTag(tagName);
            }

            SelectedTags.Add(new TagItemViewModel(tag));
            NewTagInput = string.Empty;
            SetDirty();
            
            LoadAvailableTags();
        }

        private bool CanAddTag()
        {
            return !string.IsNullOrWhiteSpace(NewTagInput);
        }

        private void RemoveTag(TagItemViewModel? tagItem)
        {
            if (tagItem == null) return;
            
            SelectedTags.Remove(tagItem);
            SetDirty();
            
            LoadAvailableTags();
        }

        private void LoadAvailableTags()
        {
            AvailableTags.Clear();
            var allTags = _tagService.GetAllTags();
            
            foreach (var tag in allTags)
            {
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
