using System;
using System.Windows.Controls;
using NotionNote.Models;
using NotionNote.Services;
using NotionNote.ViewModels;

namespace NotionNote.Views
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
            // ViewModel will be set via DataContext binding from MainWindow
        }
    }
}
