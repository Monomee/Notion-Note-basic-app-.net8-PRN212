using NotionNote.Commands;
using NotionNote.Models;
using NotionNote.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NotionNote.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoginMode = true;
        private bool _isBusy = false;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            LoginCommand = new RelayCommand(Login, CanLogin);
            RegisterCommand = new RelayCommand(Register, CanRegister);
            SwitchModeCommand = new RelayCommand(SwitchMode);
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
                    ErrorMessage = string.Empty;
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
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

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set
            {
                if (_isLoginMode != value)
                {
                    _isLoginMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ModeTitle));
                    OnPropertyChanged(nameof(ModeSwitchText));
                    ErrorMessage = string.Empty;
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

        public string ModeTitle => IsLoginMode ? "Welcome Back" : "Create Account";
        public string ModeSwitchText => IsLoginMode ? "Don't have an account? Sign up" : "Already have an account? Login";

        private User? _authenticatedUser;

        public User? AuthenticatedUser
        {
            get => _authenticatedUser;
            private set
            {
                if (_authenticatedUser != value)
                {
                    _authenticatedUser = value;
                    OnPropertyChanged();  // ← QUAN TRỌNG!
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand SwitchModeCommand { get; }

        #endregion

        #region Command Implementations

        private void Login()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var user = _authService.Login(Username, Password);

                if (user != null)
                {
                    // Debug
                    System.Diagnostics.Debug.WriteLine($"Login successful: {user.Username} (ID: {user.UserId})");

                    AuthenticatedUser = user;  // ← Bây giờ sẽ fire PropertyChanged
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsBusy;
        }

        private void Register()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (Username.Length < 3)
                {
                    ErrorMessage = "Username must be at least 3 characters";
                    return;
                }

                if (Password.Length < 3)
                {
                    ErrorMessage = "Password must be at least 3 characters";
                    return;
                }

                var user = _authService.Register(Username, Password);

                if (user != null)
                {
                    AuthenticatedUser = user;
                    // Close login window
                }
                else
                {
                    ErrorMessage = "Username already exists";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsBusy;
        }

        private void SwitchMode()
        {
            IsLoginMode = !IsLoginMode;
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
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
