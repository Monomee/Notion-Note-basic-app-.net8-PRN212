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
    public class WorkspaceItemViewModel : INotifyPropertyChanged
    {
        private readonly Workspace _workspace;
        private readonly IWorkspaceService _workspaceService;
        private bool _isEditing;
        private string _name;

        public WorkspaceItemViewModel(Workspace workspace, IWorkspaceService workspaceService)
        {
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
            _name = workspace.Name;
        }

        public Workspace Workspace => _workspace;

        public int WorkspaceId => _workspace.WorkspaceId;
        
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? CreatedAt => _workspace.CreatedAt;
        public int UserId => _workspace.UserId;
        public int PageCount => _workspace.Pages?.Count ?? 0;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                    
                    // Save changes when editing ends
                    if (!value && _workspace.Name != _name)
                    {
                        _workspace.Name = _name;
                        _workspaceService.UpdateWorkspace(_workspace);
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

    public class WorkSpaceListViewModel : INotifyPropertyChanged
    {
        private readonly IWorkspaceService _workspaceService;
        private ObservableCollection<WorkspaceItemViewModel> _workspaces = new();
        private ObservableCollection<WorkspaceItemViewModel> _filteredWorkspaces = new();
        private string _searchText = string.Empty;
        private WorkspaceItemViewModel? _selected;
        private int _currentUserId = 1; // Default user ID, should be set from authentication
        private bool _isBusy;

        public WorkSpaceListViewModel(IWorkspaceService workspaceService)
        {
            _workspaceService = workspaceService ?? throw new ArgumentNullException(nameof(workspaceService));
            
            // Initialize commands
            AddWorkspaceCommand = new RelayCommand(AddWorkspace, CanAddWorkspace);
            DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace, CanDeleteWorkspace);
            RefreshCommand = new RelayCommand(RefreshWorkspaces);
            RenameWorkspaceCommand = new RelayCommand(RenameWorkspace, CanRenameWorkspace);
            // Initialize filtered workspaces
            _filteredWorkspaces = new ObservableCollection<WorkspaceItemViewModel>(_workspaces);
        }

        #region Properties

        public ObservableCollection<WorkspaceItemViewModel> Workspaces
        {
            get => _workspaces;
            private set
            {
                if (_workspaces != value)
                {
                    _workspaces = value;
                    OnPropertyChanged();
                    UpdateFilteredWorkspaces();
                }
            }
        }

        public ObservableCollection<WorkspaceItemViewModel> FilteredWorkspaces
        {
            get => _filteredWorkspaces;
            private set
            {
                if (_filteredWorkspaces != value)
                {
                    _filteredWorkspaces = value;
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
                    UpdateFilteredWorkspaces();
                }
            }
        }

        public WorkspaceItemViewModel? Selected
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

        public int CurrentUserId
        {
            get => _currentUserId;
            set
            {
                if (_currentUserId != value)
                {
                    _currentUserId = value;
                    OnPropertyChanged();
                    RefreshWorkspaces();
                }
            }
        }

        public bool IsEmpty
        {
            get => _filteredWorkspaces.Count == 0;
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

        public ICommand AddWorkspaceCommand { get; }
        public ICommand DeleteWorkspaceCommand { get; }
        public ICommand RefreshCommand { get; }

        public ICommand RenameWorkspaceCommand { get; }

        #endregion

        #region Command Implementations

        private void AddWorkspace()
        {
            IsBusy = true;
            try
            {
                var newWorkspace = new Workspace
                {
                    Name = "New Workspace",
                    CreatedAt = DateTime.Now,
                    UserId = CurrentUserId
                };

                var createdWorkspace = _workspaceService.CreateWorkspace(newWorkspace);
                var workspaceItem = new WorkspaceItemViewModel(createdWorkspace, _workspaceService);
                
                _workspaces.Insert(0, workspaceItem);
                UpdateFilteredWorkspaces();
                Selected = workspaceItem;
                workspaceItem.IsEditing = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanAddWorkspace()
        {
            return !IsBusy;
        }

        private void DeleteWorkspace()
        {
            if (Selected == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete workspace '{Selected.Name}'?\nAll pages in this workspace will also be deleted.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            IsBusy = true;
            try
            {
                _workspaceService.DeleteWorkspace(Selected.WorkspaceId);
                _workspaces.Remove(Selected);
                UpdateFilteredWorkspaces();
                Selected = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanDeleteWorkspace()
        {
            return Selected != null && !IsBusy;
        }

        private void RefreshWorkspaces()
        {
            IsBusy = true;
            try
            {
                var workspaces = _workspaceService.GetWorkspacesByUserId(CurrentUserId);
                _workspaces.Clear();
                
                foreach (var workspace in workspaces)
                {
                    _workspaces.Add(new WorkspaceItemViewModel(workspace, _workspaceService));
                }
                
                UpdateFilteredWorkspaces();
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void RenameWorkspace()
        {
            if (Selected != null)
            {
                Selected.IsEditing = true;
            }
        }

        private bool CanRenameWorkspace()
        {
            return Selected != null && !IsBusy;
        }

        #endregion

        #region Helper Methods

        private void UpdateFilteredWorkspaces()
        {
            var filtered = _workspaces.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(w => w.Name.ToLower().Contains(searchLower));
            }

            _filteredWorkspaces.Clear();
            foreach (var item in filtered)
            {
                _filteredWorkspaces.Add(item);
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
