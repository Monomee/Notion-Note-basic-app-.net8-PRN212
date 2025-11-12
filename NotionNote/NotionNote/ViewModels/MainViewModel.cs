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
        private readonly int _currentUserId;
        public MainViewModel(int userId)
        {
            _currentUserId = userId;  // ? LƯU userId

            try
            {
                // Initialize DbContext and services
                _dbContext = new NoteHubDbContext();
                _pageService = new PageService(_dbContext);
                _workspaceService = new WorkspaceService(_dbContext);

                // Initialize ViewModels
                WorkSpaceListVM = new WorkSpaceListViewModel(_workspaceService);
                PageListVM = new PageListViewModel(_pageService);
                EditorVM = new EditorViewModel(_pageService);

                // Set up event handlers
                WorkSpaceListVM.PropertyChanged += WorkSpaceListVM_PropertyChanged;
                PageListVM.PropertyChanged += PageListVM_PropertyChanged;
                EditorVM.PageUpdated += EditorVM_PageUpdated;
                
                // Initialize commands
                LogoutCommand = new RelayCommand(Logout);
                
                // Load initial data
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
                }
                else
                {
                    PageListVM.CurrentWorkspaceId = null;
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

        #endregion

        #region Helper Methods

        private void LoadInitialData()
    {
        // Load workspaces for the logged-in user
        WorkSpaceListVM.CurrentUserId = _currentUserId;  // ? S? D?NG userId
        
        WorkSpaceListVM.RefreshCommand.Execute(null);
        
        if (WorkSpaceListVM.FilteredWorkspaces.Count > 0)
        {
            WorkSpaceListVM.Selected = WorkSpaceListVM.FilteredWorkspaces[0];
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
}

