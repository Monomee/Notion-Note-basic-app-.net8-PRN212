using System;
using System.Globalization;
using System.Windows.Data;

namespace NotionNote.Converters
{
    public class SidebarTotalWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? 320.0 : 40.0;
            }
            return 40.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

