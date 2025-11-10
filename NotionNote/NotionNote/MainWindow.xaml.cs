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
            DataContext = new MainViewModel(userId);  // ← TRUYỀN userId
        }
    }
}