using System.Windows;
using System.Windows.Controls;
using NotionNote.ViewModels;

namespace NotionNote.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && DataContext is SettingsViewModel viewModel)
            {
                if (radioButton.Content.ToString()?.Contains("Dark") == true)
                {
                    viewModel.IsDarkMode = true;
                }
                else
                {
                    viewModel.IsDarkMode = false;
                }
            }
        }
    }
}

