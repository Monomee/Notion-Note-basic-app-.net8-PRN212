using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NotionNote.ViewModels;
using NotionNote.Views;

namespace NotionNote
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(int userId)  // ← THÊM PARAMETER
        {
            InitializeComponent();
            InitializeDataContext(userId);  // ← TRUYỀN userId
        }

        private void InitializeDataContext(int userId)  // ← THÊM PARAMETER
        {
            // Create MainViewModel with userId
            var viewModel = new MainViewModel(userId);  // ← TRUYỀN userId
            DataContext = viewModel;
            
            // Subscribe to logout event
            viewModel.LogoutRequested += ViewModel_LogoutRequested;
        }

        private void ViewModel_LogoutRequested(object? sender, EventArgs e)
        {
            // Change shutdown mode to prevent auto-shutdown when closing MainWindow
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            // Close MainWindow
            this.Close();
            
            // Show login window again
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();
            
            if (result == true)
            {
                var user = loginWindow.AuthenticatedUser;
                if (user != null)
                {
                    // Change shutdown mode back before showing new MainWindow
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    
                    // Create new MainWindow with new user
                    var newMainWindow = new MainWindow(user.UserId);
                    Application.Current.MainWindow = newMainWindow;
                    newMainWindow.Show();
                }
                else
                {
                    // User closed login window, shutdown app
                    Application.Current.Shutdown();
                }
            }
            else
            {
                // User closed login window, shutdown app
                Application.Current.Shutdown();
            }
        }
    }
}