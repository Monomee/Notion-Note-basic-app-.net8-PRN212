using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotionNote.ViewModels;

namespace NotionNote.Views
{
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is EditorViewModel viewModel)
            {
                if (viewModel.AddTagCommand.CanExecute(null))
                {
                    viewModel.AddTagCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is TagItemViewModel tagItem && DataContext is EditorViewModel viewModel)
            {
                if (!viewModel.SelectedTags.Any(t => t.TagId == tagItem.TagId))
                {
                    viewModel.SelectedTags.Add(tagItem);
                    viewModel.SetDirty();
                    viewModel.NewTagInput = string.Empty;
                    comboBox.SelectedItem = null;
                    comboBox.Text = string.Empty;
                }
            }
        }
    }
}
