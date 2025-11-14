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
        private bool _isBusy = false;

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

        public bool IsBusy
        {
            get => _isBusy;
            set
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

        public ICommand ChangePasswordCommand { get; }

        #endregion

        #region Command Implementations

        private void ChangePassword()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(OldPassword))
                {
                    ErrorMessage = "Vui lòng nhập mật khẩu cũ";
                    return;
                }

                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    ErrorMessage = "Vui lòng nhập mật khẩu mới";
                    return;
                }

                if (NewPassword.Length < 3)
                {
                    ErrorMessage = "Mật khẩu mới phải có ít nhất 3 ký tự";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không khớp";
                    return;
                }

                bool success = _authService.ChangePassword(_userId, OldPassword, NewPassword);

                if (success)
                {
                    SuccessMessage = "Đổi mật khẩu thành công!";
                    OldPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;

                    MessageBox.Show(
                        "Đổi mật khẩu thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Mật khẩu cũ không đúng";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(OldPassword) &&
                   !string.IsNullOrWhiteSpace(NewPassword) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !IsBusy;
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
                ErrorMessage = $"Không thể tải thông tin người dùng: {ex.Message}";
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

