using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using NotionNote.ViewModels;

namespace NotionNote.Converters
{
    public class ContentVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string param)
            {
                // Handle SidebarContentType
                if (value is SidebarContentType contentType)
                {
                    if (Enum.TryParse<SidebarContentType>(param, out var expectedType))
                    {
                        return contentType == expectedType ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                // Handle MainViewType
                else if (value is MainViewType viewType)
                {
                    if (Enum.TryParse<MainViewType>(param, out var expectedType))
                    {
                        return viewType == expectedType ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

