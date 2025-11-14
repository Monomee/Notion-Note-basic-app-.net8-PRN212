using System.Configuration;
using System.Data;
using System.Windows;
using NotionNote.Models;
using NotionNote.Data;
using NotionNote.Views;

namespace NotionNote
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);


            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                using (var context = new NoteHubDbContext())
                {
                    DatabaseSeeder.Seed(context);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Database initialization error:\n{ex.Message}",
                               "Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result == true)
            {
                var user = loginWindow.AuthenticatedUser;
                if (user != null)
                {
                    try
                    {
                        this.ShutdownMode = ShutdownMode.OnMainWindowClose;

                        var mainWindow = new MainWindow(user.UserId);
                        this.MainWindow = mainWindow;
                        mainWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open MainWindow:\n\n" +
                                      $"Message: {ex.Message}\n\n" +
                                      $"Inner: {ex.InnerException?.Message}",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        Shutdown();
                    }
                }
                else
                {
                    Shutdown();
                }
            }
            else
            {
                Shutdown();
            }
        }
    }
}