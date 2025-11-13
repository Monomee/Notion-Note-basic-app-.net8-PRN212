using NotionNote.Commands;
using NotionNote.Models;
using NotionNote.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NotionNote.ViewModels
{
    public class TrashItemViewModel : INotifyPropertyChanged
    {
        private readonly object _item; // Can be Page or Workspace
        private readonly string _type; // "Page" or "Workspace"
        private readonly int _id;
        private readonly string _name;
        private readonly DateTime? _deletedAt;
        private readonly bool _isRestoreDisabled; // True if page belongs to deleted workspace
        private readonly string? _workspaceName; // Name of workspace if page belongs to deleted workspace

        public TrashItemViewModel(Page page, bool isRestoreDisabled = false, string? workspaceName = null)
        {
            _item = page;
            _type = "Page";
            _id = page.PageId;
            _name = page.Title;
            _deletedAt = page.UpdatedAt ?? page.CreatedAt;
            _isRestoreDisabled = isRestoreDisabled;
            _workspaceName = workspaceName;
        }

        public TrashItemViewModel(Workspace workspace)
        {
            _item = workspace;
            _type = "Workspace";
            _id = workspace.WorkspaceId;
            _name = workspace.Name;
            _deletedAt = workspace.CreatedAt;
            _isRestoreDisabled = false; // Workspaces can always be restored
        }

        public string Type => _type;
        public int Id => _id;
        public string Name => _name;
        public DateTime? DeletedAt => _deletedAt;
        public object Item => _item;
        public bool IsRestoreDisabled => _isRestoreDisabled;
        public string? WorkspaceName => _workspaceName;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TrashViewModel : INotifyPropertyChanged
    {
        private readonly IPageService _pageService;
        private readonly IWorkspaceService _workspaceService;
        private readonly int _userId;
        private ObservableCollection<TrashItemViewModel> _items = new();
        private TrashItemViewModel? _selected;
        private bool _isBusy;
        private HashSet<int> _deletedWorkspaceIds = new(); // Track deleted workspace IDs

        public TrashViewModel(IPageService pageService, IWorkspaceService workspaceService, int userId)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
            _userId = userId;

            RestoreCommand = new RelayCommand(RestoreItem, CanRestoreItem);
            HardDeleteCommand = new RelayCommand(HardDeleteItem, CanHardDeleteItem);
            RefreshCommand = new RelayCommand(RefreshItems);

            RefreshItems();
        }

        #region Properties

        public ObservableCollection<TrashItemViewModel> Items
        {
            get => _items;
            private set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrashItemViewModel? Selected
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

        public ICommand RestoreCommand { get; }
        public ICommand HardDeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Events

        public event EventHandler? ItemRestored;

        #endregion

        #region Command Implementations

        private void RestoreItem()
        {
            if (Selected == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to restore '{Selected.Name}'?",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            IsBusy = true;
            try
            {
                string restoredType = Selected.Type;
                int restoredId = Selected.Id;
                
                if (Selected.Type == "Page")
                {
                    _pageService.RestorePage(Selected.Id);
                }
                else if (Selected.Type == "Workspace")
                {
                    _workspaceService.RestoreWorkspace(Selected.Id);
                }

                RefreshItems();
                Selected = null;
                
                // Notify that an item was restored
                ItemRestored?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRestoreItem()
        {
            // Cannot restore if:
            // 1. Nothing selected
            // 2. Busy
            // 3. Selected item is a page that belongs to a deleted workspace
            if (Selected == null || IsBusy)
                return false;
            
            return !Selected.IsRestoreDisabled;
        }

        private void HardDeleteItem()
        {
            if (Selected == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to permanently delete '{Selected.Name}'?\nThis action cannot be undone!",
                "Confirm Permanent Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            IsBusy = true;
            try
            {
                if (Selected.Type == "Page")
                {
                    _pageService.HardDeletePage(Selected.Id);
                }
                else if (Selected.Type == "Workspace")
                {
                    _workspaceService.HardDeleteWorkspace(Selected.Id);
                }

                RefreshItems();
                Selected = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanHardDeleteItem()
        {
            return Selected != null && !IsBusy;
        }

        private void RefreshItems()
        {
            IsBusy = true;
            try
            {
                _items.Clear();
                _deletedWorkspaceIds.Clear();

                // Load deleted workspaces first to track which workspaces are deleted
                var deletedWorkspaces = _workspaceService.GetDeletedWorkspaces(_userId);
                foreach (var workspace in deletedWorkspaces)
                {
                    _deletedWorkspaceIds.Add(workspace.WorkspaceId);
                    _items.Add(new TrashItemViewModel(workspace));
                }

                // Load deleted pages
                var deletedPages = _pageService.GetDeletedPages(_userId);
                foreach (var page in deletedPages)
                {
                    // Check if page's workspace is also deleted
                    bool isRestoreDisabled = _deletedWorkspaceIds.Contains(page.WorkspaceId);
                    string? workspaceName = null;
                    
                    if (isRestoreDisabled)
                    {
                        // Find the workspace name from deleted workspaces
                        var deletedWorkspace = deletedWorkspaces.FirstOrDefault(w => w.WorkspaceId == page.WorkspaceId);
                        workspaceName = deletedWorkspace?.Name;
                    }
                    
                    _items.Add(new TrashItemViewModel(page, isRestoreDisabled, workspaceName));
                }

                // Sort by deleted date (newest first)
                var sorted = _items.OrderByDescending(i => i.DeletedAt).ToList();
                _items.Clear();
                foreach (var item in sorted)
                {
                    _items.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
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

