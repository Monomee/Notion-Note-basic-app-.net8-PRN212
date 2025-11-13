using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NotionNote.Helpers;

namespace NotionNote.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private bool _isDarkMode = false;

        public SettingsViewModel()
        {
            // Load saved theme preference
            LoadThemePreference();
        }

        #region Properties

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged();
                    ApplyTheme();
                    SaveThemePreference();
                }
            }
        }

        #endregion

        #region Helper Methods

        private void ApplyTheme()
        {
            ThemeManager.ApplyTheme(IsDarkMode);
        }

        private void LoadThemePreference()
        {
            // Try to load from settings (for now, default to light)
            // In the future, can load from appsettings.json or user settings
            IsDarkMode = false;
        }

        private void SaveThemePreference()
        {
            // Save theme preference (can be saved to appsettings.json or user settings)
            // For now, just apply the theme
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

