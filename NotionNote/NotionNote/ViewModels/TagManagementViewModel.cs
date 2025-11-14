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
    public class TagManagementViewModel : INotifyPropertyChanged
    {
        private readonly ITagService _tagService;
        private string _searchText = string.Empty;
        private string _newTagName = string.Empty;
        private string _errorMessage = string.Empty;
        private TagItemViewModel? _selectedTag;

        public TagManagementViewModel(ITagService tagService)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));

            Tags = new ObservableCollection<TagItemViewModel>();
            FilteredTags = new ObservableCollection<TagItemViewModel>();

            AddTagCommand = new RelayCommand(AddTag, CanAddTag);
            DeleteTagCommand = new RelayCommand(DeleteTag, CanDeleteTag);
            RefreshCommand = new RelayCommand(RefreshTags);

            LoadTags();
        }

        #region Properties

        public ObservableCollection<TagItemViewModel> Tags { get; }
        public ObservableCollection<TagItemViewModel> FilteredTags { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    UpdateFilteredTags();
                }
            }
        }

        public string NewTagName
        {
            get => _newTagName;
            set
            {
                if (_newTagName != value)
                {
                    _newTagName = value;
                    OnPropertyChanged();
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public TagItemViewModel? SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddTagCommand { get; }
        public ICommand DeleteTagCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Command Implementations

        private void AddTag()
        {
            ErrorMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(NewTagName))
                {
                    ErrorMessage = "Tag name cannot be empty";
                    return;
                }

                if (NewTagName.Length > 50)
                {
                    ErrorMessage = "Tag name cannot exceed 50 characters";
                    return;
                }

                var tag = _tagService.CreateTag(NewTagName.Trim());
                var tagItem = new TagItemViewModel(tag);
                Tags.Add(tagItem);
                UpdateFilteredTags();
                NewTagName = string.Empty;

                MessageBox.Show(
                    $"Tag '{tag.Name}' has been created successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private bool CanAddTag()
        {
            return !string.IsNullOrWhiteSpace(NewTagName);
        }

        private void DeleteTag()
        {
            if (SelectedTag == null)
                return;

            ErrorMessage = string.Empty;

            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete tag '{SelectedTag.Name}'?\n\nNote: The tag will be removed from all pages that have this tag attached.",
                    "Confirm Tag Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _tagService.DeleteTag(SelectedTag.TagId);
                    Tags.Remove(SelectedTag);
                    UpdateFilteredTags();
                    SelectedTag = null;

                    MessageBox.Show(
                        "Tag has been deleted successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private bool CanDeleteTag()
        {
            return SelectedTag != null;
        }

        private void RefreshTags()
        {
            LoadTags();
        }

        #endregion

        #region Helper Methods

        private void LoadTags()
        {
            ErrorMessage = string.Empty;

            try
            {
                Tags.Clear();
                var tags = _tagService.GetAllTags();

                foreach (var tag in tags)
                {
                    Tags.Add(new TagItemViewModel(tag));
                }

                UpdateFilteredTags();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unable to load tags: {ex.Message}";
            }
        }

        private void UpdateFilteredTags()
        {
            FilteredTags.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Tags
                : Tags.Where(t => t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var tag in filtered.OrderBy(t => t.Name))
            {
                FilteredTags.Add(tag);
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

    public class TagItemViewModel : INotifyPropertyChanged
    {
        private readonly Tag _tag;

        public TagItemViewModel(Tag tag)
        {
            _tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }

        public int TagId => _tag.TagId;
        public string Name => _tag.Name;
        public int PageCount => _tag.Pages?.Count ?? 0;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}

