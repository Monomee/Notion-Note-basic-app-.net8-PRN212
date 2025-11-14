using NotionNote.Commands;
using NotionNote.Models;
using NotionNote.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NotionNote.ViewModels
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private readonly int _userId;
        private string _username = string.Empty;
        private string _oldPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;

        public UserProfileViewModel(IAuthService authService, int userId)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userId = userId;

            ChangePasswordCommand = new RelayCommand(ChangePassword, CanChangePassword);
            LoadUserInfo();
        }

        #region Properties

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

        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                if (_oldPassword != value)
                {
                    _oldPassword = value;
                    OnPropertyChanged();
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (_newPassword != value)
                {
                    _newPassword = value;
                    OnPropertyChanged();
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
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

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                if (_successMessage != value)
                {
                    _successMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ChangePasswordCommand { get; }

        #endregion

        #region Command Implementations

        private void ChangePassword()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(OldPassword))
                {
                    ErrorMessage = "Please enter your old password";
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    ErrorMessage = "Please enter your new password";
                    return;
                }

                if (NewPassword.Length < 3)
                {
                    ErrorMessage = "New password must be at least 3 characters";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    ErrorMessage = "New password and confirm password do not match";
                    return;
                }

                bool success = _authService.ChangePassword(_userId, OldPassword, NewPassword);

                if (success)
                {
                    SuccessMessage = "Password changed successfully!";
                    OldPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;

                    MessageBox.Show(
                        "Password changed successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Old password is incorrect";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(OldPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
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
                ErrorMessage = $"Unable to load user information: {ex.Message}";
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

