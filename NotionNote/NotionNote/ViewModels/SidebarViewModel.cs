using NotionNote.Commands;
using NotionNote.Models;
using NotionNote.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NotionNote.ViewModels
{
    public class SidebarViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly int _userId;
        private readonly ITagService _tagService;
        private bool _isExpanded = false;
        private string _username = string.Empty;
        private SidebarContentType _currentContent = SidebarContentType.None;

        public SidebarViewModel(IAuthService authService, ITagService tagService, int userId)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            _userId = userId;

            ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
            ShowSettingsCommand = new RelayCommand(() => 
            {
                // Toggle: nếu đang ở Settings thì quay về Main, nếu không thì chuyển sang Settings
                CurrentContent = CurrentContent == SidebarContentType.Settings 
                    ? SidebarContentType.None 
                    : SidebarContentType.Settings;
            });
            ShowUserProfileCommand = new RelayCommand(() => 
            {
                CurrentContent = CurrentContent == SidebarContentType.UserProfile 
                    ? SidebarContentType.None 
                    : SidebarContentType.UserProfile;
            });
            ShowTagManagementCommand = new RelayCommand(() => 
            {
                CurrentContent = CurrentContent == SidebarContentType.TagManagement 
                    ? SidebarContentType.None 
                    : SidebarContentType.TagManagement;
            });
            ShowTrashCommand = new RelayCommand(() => 
            {
                CurrentContent = CurrentContent == SidebarContentType.Trash 
                    ? SidebarContentType.None 
                    : SidebarContentType.Trash;
            });
            ShowMainCommand = new RelayCommand(() => CurrentContent = SidebarContentType.None);
            CloseContentCommand = new RelayCommand(() => CurrentContent = SidebarContentType.None);

            LoadUserInfo();
        }

        #region Properties

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public SidebarContentType CurrentContent
        {
            get => _currentContent;
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged();
                    // Auto expand when content is shown
                    if (value != SidebarContentType.None)
                    {
                        IsExpanded = true;
                    }
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleSidebarCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowUserProfileCommand { get; }
        public ICommand ShowTagManagementCommand { get; }
        public ICommand ShowTrashCommand { get; }
        public ICommand ShowMainCommand { get; }
        public ICommand CloseContentCommand { get; }
        public ICommand? LogoutCommand { get; set; }

        #endregion

        #region Command Implementations

        private void ToggleSidebar()
        {
            IsExpanded = !IsExpanded;
            // If collapsing, also close content
            if (!IsExpanded)
            {
                CurrentContent = SidebarContentType.None;
            }
        }

        #endregion

        #region Helper Methods

        private void LoadUserInfo()
        {
            try
            {
                var user = _authService.GetUserById(_userId);
                if (user != null)
                {
                    Username = user.Username;
                }
            }
            catch (Exception ex)
            {
                // Silently fail - username will remain empty
                System.Diagnostics.Debug.WriteLine($"Failed to load user info: {ex.Message}");
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

    public enum SidebarContentType
    {
        None,
        Settings,
        UserProfile,
        TagManagement,
        Trash
    }
}

