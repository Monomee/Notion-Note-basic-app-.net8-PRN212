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
    public partial class WorkSpaceListView : UserControl
    {
        public WorkSpaceListView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
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
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
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
