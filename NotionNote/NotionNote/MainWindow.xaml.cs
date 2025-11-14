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
    public partial class MainWindow : Window
    {
        public MainWindow(int userId)  
        {
            InitializeComponent();
            InitializeDataContext(userId);  
        }

        private void InitializeDataContext(int userId)  
        {
            var viewModel = new MainViewModel(userId); 
            DataContext = viewModel;
            
            viewModel.LogoutRequested += ViewModel_LogoutRequested;
        }

        private void ViewModel_LogoutRequested(object? sender, EventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            this.Close();
            
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();
            
            if (result == true)
            {
                var user = loginWindow.AuthenticatedUser;
                if (user != null)
                {
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    
                    var newMainWindow = new MainWindow(user.UserId);
                    Application.Current.MainWindow = newMainWindow;
                    newMainWindow.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}