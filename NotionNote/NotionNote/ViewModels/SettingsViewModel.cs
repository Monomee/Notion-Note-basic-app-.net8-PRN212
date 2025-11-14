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
            IsDarkMode = false;
        }

        private void SaveThemePreference()
        {
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

