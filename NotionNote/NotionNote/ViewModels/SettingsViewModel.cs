using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotionNote.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            // Settings can be added here in the future
            // For now, this is a placeholder
        }

        #region Properties

        // Add settings properties here as needed
        // Example:
        // public bool DarkMode { get; set; }
        // public int FontSize { get; set; }

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

