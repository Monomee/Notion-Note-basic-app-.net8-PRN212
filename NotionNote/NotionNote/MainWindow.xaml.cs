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
        public MainWindow()
        {
            InitializeComponent();
            InitializeDataContext();
        }

        private void InitializeDataContext()
        {
            // Create MainViewModel which will initialize all ViewModels and load data
            DataContext = new MainViewModel();
        }
    }
}