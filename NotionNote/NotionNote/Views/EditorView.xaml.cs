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
            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            // Create DbContext and services
            var dbContext = new NoteHubDbContext();
            var pageService = new PageService(dbContext);
            
            // Create and set ViewModel
            var viewModel = new EditorViewModel(pageService);
            DataContext = viewModel;
        }
    }
}
