using NotionNote.Models;
using NotionNote.Services;
using NotionNote.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NotionNote.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            // Initialize services
            var context = new NoteHubDbContext();
            var authService = new AuthService(context);

            _viewModel = new LoginViewModel(authService);
            DataContext = _viewModel;

            // Subscribe to property changed to close window on success
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public User? AuthenticatedUser => _viewModel.AuthenticatedUser;

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.AuthenticatedUser))
            {
                if (_viewModel.AuthenticatedUser != null)
                {
                    // CHỈ SET DialogResult - KHÔNG GỌI Close()
                    this.DialogResult = true;
                    // Close() sẽ tự động được gọi khi set DialogResult = true
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }
    }
}
