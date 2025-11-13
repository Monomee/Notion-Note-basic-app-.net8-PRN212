using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using NotionNote.Models;
using NotionNote.Services;
using NotionNote.ViewModels;
using NotionNote.Commands;
using System.Windows.Input;
using System.Windows;

namespace NotionNote.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly NoteHubDbContext _dbContext;
        private readonly IPageService _pageService;
        private readonly IWorkspaceService _workspaceService;
        private readonly IAuthService _authService;
        private readonly ITagService _tagService;
        private readonly int _currentUserId;
        private MainViewType _currentView = MainViewType.Main;
        
        public MainViewModel(int userId)
        {
            _currentUserId = userId;  

            try
            {
                
                _dbContext = new NoteHubDbContext();
                _pageService = new PageService(_dbContext);
                _workspaceService = new WorkspaceService(_dbContext);
                _authService = new AuthService(_dbContext);
                _tagService = new TagService(_dbContext);

            
                WorkSpaceListVM = new WorkSpaceListViewModel(_workspaceService);
                PageListVM = new PageListViewModel(_pageService);
                EditorVM = new EditorViewModel(_pageService, _tagService);
                SidebarVM = new SidebarViewModel(_authService, _tagService, _currentUserId);
                
                // Set LogoutCommand for Sidebar
                LogoutCommand = new RelayCommand(Logout);
                SidebarVM.LogoutCommand = LogoutCommand;

                // Initialize child ViewModels for settings views
                SettingsVM = new SettingsViewModel();
                UserProfileVM = new UserProfileViewModel(_authService, _currentUserId);
                TagManagementVM = new TagManagementViewModel(_tagService);
                TrashVM = new TrashViewModel(_pageService, _workspaceService, _currentUserId);

               
                WorkSpaceListVM.PropertyChanged += WorkSpaceListVM_PropertyChanged;
                PageListVM.PropertyChanged += PageListVM_PropertyChanged;
                EditorVM.PageUpdated += EditorVM_PageUpdated;
                SidebarVM.PropertyChanged += SidebarVM_PropertyChanged;
                TrashVM.ItemRestored += TrashVM_ItemRestored;
                
             
                LoadInitialData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MainViewModel initialization failed:\n{ex.Message}\n\nStack:\n{ex.StackTrace}");
                throw;
            }
        }

        #region Properties

        public WorkSpaceListViewModel WorkSpaceListVM { get; }
        public PageListViewModel PageListVM { get; }
        public EditorViewModel EditorVM { get; }
        public SidebarViewModel SidebarVM { get; }
        public SettingsViewModel SettingsVM { get; }
        public UserProfileViewModel UserProfileVM { get; }
        public TagManagementViewModel TagManagementVM { get; }
        public TrashViewModel TrashVM { get; }

        public MainViewType CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LogoutCommand { get; }

        #endregion

        #region Events

        public event EventHandler? LogoutRequested;

        #endregion

        #region Event Handlers

        private void WorkSpaceListVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When a workspace is selected, update the PageListViewModel's CurrentWorkspaceId
            if (e.PropertyName == nameof(WorkSpaceListViewModel.Selected))
            {
                // Clear current page selection when switching workspace
                EditorVM.CurrentPage = null;
                
                if (WorkSpaceListVM.Selected != null)
                {
                    PageListVM.CurrentWorkspaceId = WorkSpaceListVM.Selected.WorkspaceId;
                    EditorVM.CurrentWorkspaceId = WorkSpaceListVM.Selected.WorkspaceId;
                }
                else
                {
                    PageListVM.CurrentWorkspaceId = null;
                    EditorVM.CurrentWorkspaceId = null;
                }
            }
        }

        private void PageListVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When a page is selected, update the EditorViewModel's CurrentPage
            if (e.PropertyName == nameof(PageListViewModel.Selected))
            {
                if (PageListVM.Selected != null)
                {
                    // Load the full page data from the service to get all content
                    var fullPage = _pageService.GetPageById(PageListVM.Selected.PageId);
                    EditorVM.CurrentPage = fullPage;
                }
                else
                {
                    EditorVM.CurrentPage = null;
                }
            }
            // When workspace changes, update EditorViewModel's CurrentWorkspaceId
            else if (e.PropertyName == nameof(PageListViewModel.CurrentWorkspaceId))
            {
                EditorVM.CurrentWorkspaceId = PageListVM.CurrentWorkspaceId;
            }
        }
        private void EditorVM_PageUpdated(object? sender, Page updatedPage)
        {
            // Update page list to reflect changes (pin/unpin, title, etc.)
            if (PageListVM.CurrentWorkspaceId.HasValue)
            {
                // Find the page item in the current list
                var existingItem = PageListVM.Pages.FirstOrDefault(p => p.PageId == updatedPage.PageId);
                
                if (existingItem != null)
                {
                    // Update the underlying page data
                    var page = existingItem.Page;
                    page.Title = updatedPage.Title;
                    page.IsPinned = updatedPage.IsPinned;
                    page.UpdatedAt = updatedPage.UpdatedAt;
                    
                    // Update the view model properties
                    existingItem.Title = updatedPage.Title;
                    
                    // Re-sort the filtered pages (pinned pages will move to top)
                    PageListVM.UpdateFilteredPages();
                    
                    // Re-select the updated page
                    PageListVM.Selected = existingItem;
                }
                else
                {
                    // If page not found, refresh the entire list
                    PageListVM.RefreshCommand.Execute(null);
                    
                    // Re-select the updated page
                    var updatedItem = PageListVM.FilteredPages
                        .FirstOrDefault(p => p.PageId == updatedPage.PageId);
                    if (updatedItem != null)
                    {
                        PageListVM.Selected = updatedItem;
                    }
                }
            }
        }

        private void SidebarVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.CurrentContent))
            {
                // Map SidebarContentType to MainViewType
                switch (SidebarVM.CurrentContent)
                {
                    case SidebarContentType.None:
                        CurrentView = MainViewType.Main;
                        break;
                    case SidebarContentType.Settings:
                        CurrentView = MainViewType.Settings;
                        break;
                    case SidebarContentType.UserProfile:
                        CurrentView = MainViewType.UserProfile;
                        break;
                    case SidebarContentType.TagManagement:
                        CurrentView = MainViewType.TagManagement;
                        break;
                    case SidebarContentType.Trash:
                        CurrentView = MainViewType.Trash;
                        break;
                }
            }
        }

        private void TrashVM_ItemRestored(object? sender, EventArgs e)
        {
            // Refresh workspaces and pages when an item is restored from trash
            WorkSpaceListVM.RefreshCommand.Execute(null);
            
            // If a workspace is currently selected, refresh its pages
            if (PageListVM.CurrentWorkspaceId.HasValue)
            {
                PageListVM.RefreshCommand.Execute(null);
            }
        }

        #endregion

        #region Helper Methods

        private void LoadInitialData()
    {
        // Load workspaces for the logged-in user
        WorkSpaceListVM.CurrentUserId = _currentUserId;
        
        WorkSpaceListVM.RefreshCommand.Execute(null);
        
        if (WorkSpaceListVM.FilteredWorkspaces.Count > 0)
        {
            WorkSpaceListVM.Selected = WorkSpaceListVM.FilteredWorkspaces[0];
            // This will trigger WorkSpaceListVM_PropertyChanged which sets EditorVM.CurrentWorkspaceId
        }
    }

        private void Logout()
        {
            // Show confirmation popup
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Fire event to notify MainWindow to handle logout
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            }
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

    public enum MainViewType
    {
        Main,
        Settings,
        UserProfile,
        TagManagement,
        Trash
    }
}

