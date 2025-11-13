using System.Windows;
using System.Windows.Controls;
using NotionNote.ViewModels;

namespace NotionNote.Views
{
    public partial class UserProfileView : UserControl
    {
        public UserProfileView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserProfileViewModel viewModel)
            {
                var passwordBox = sender as PasswordBox;
                if (passwordBox == null) return;

                if (passwordBox.Name == "OldPasswordBox")
                {
                    viewModel.OldPassword = passwordBox.Password;
                }
                else if (passwordBox.Name == "NewPasswordBox")
                {
                    viewModel.NewPassword = passwordBox.Password;
                }
                else if (passwordBox.Name == "ConfirmPasswordBox")
                {
                    viewModel.ConfirmPassword = passwordBox.Password;
                }
            }
        }
    }
}

