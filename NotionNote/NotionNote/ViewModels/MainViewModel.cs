using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NotionNote.Models;
using NotionNote.Services;
using NotionNote.ViewModels;
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

        #endregion

        #region Event Handlers

        private void WorkSpaceListVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When a workspace is selected, update the PageListViewModel's CurrentWorkspaceId
            if (e.PropertyName == nameof(WorkSpaceListViewModel.Selected))
            {
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
            // Refresh page list để update icon và re-sort
            if (PageListVM.CurrentWorkspaceId.HasValue)
            {
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

