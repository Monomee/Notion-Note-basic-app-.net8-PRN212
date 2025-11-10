using System;
using System.ComponentModel;
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

        private readonly IPageService _pageService;
        public event EventHandler<Page>? PageUpdated;
        public EditorViewModel(IPageService pageService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            
            // Initialize commands
            PinCommand = new RelayCommand(PinPage, CanPinPage);
            DeleteCommand = new RelayCommand(DeletePage, CanDeletePage);
            SaveCommand = new RelayCommand(SavePage, CanSavePage);
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

        #endregion

        #region Commands

        public ICommand PinCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

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
                    _pageService.UpdatePage(CurrentPage);
                    LastSavedAt = DateTime.Now;
                    IsDirty = false;
                }
                else
                {
                    var newPage = new Page
                    {
                        Title = string.IsNullOrWhiteSpace(Title) ? "Untitled Page" : Title,
                        Content = Content,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        IsPinned = false,
                        WorkspaceId = 1
                    };
                    CurrentPage = _pageService.CreatePage(newPage);
                    LastSavedAt = DateTime.Now;
                    IsDirty = false;
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
                IsDirty = false;
            }
            else
            {
                ClearPage();
            }
        }

        private void ClearPage()
        {
            Title = string.Empty;
            Content = string.Empty;
            CurrentPage = null;
            LastSavedAt = null;
            IsDirty = false;
        }

        private void SetDirty()
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
