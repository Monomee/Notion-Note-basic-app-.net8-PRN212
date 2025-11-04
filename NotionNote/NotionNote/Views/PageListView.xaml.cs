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
    /// Interaction logic for PageListView.xaml
    /// </summary>
    public partial class PageListView : UserControl
    {
        public PageListView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Handle double-click to open page
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                // This will be handled by the ViewModel through binding
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection change is handled automatically by TwoWay binding to Selected property
            // This handler is here for potential future logic (e.g., logging, additional actions)
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // End editing mode
                var dataContext = textBox.DataContext;
                if (dataContext != null)
                {
                    var isEditingProperty = dataContext.GetType().GetProperty("IsEditing");
                    if (isEditingProperty != null)
                    {
                        isEditingProperty.SetValue(dataContext, false);
                    }
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.Key == Key.Enter)
                {
                    // End editing mode on Enter
                    var dataContext = textBox.DataContext;
                    if (dataContext != null)
                    {
                        var isEditingProperty = dataContext.GetType().GetProperty("IsEditing");
                        if (isEditingProperty != null)
                        {
                            isEditingProperty.SetValue(dataContext, false);
                        }
                    }
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
                else if (e.Key == Key.Escape)
                {
                    // Cancel editing on Escape
                    var dataContext = textBox.DataContext;
                    if (dataContext != null)
                    {
                        var isEditingProperty = dataContext.GetType().GetProperty("IsEditing");
                        if (isEditingProperty != null)
                        {
                            isEditingProperty.SetValue(dataContext, false);
                        }
                    }
                    textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }
    }
}
