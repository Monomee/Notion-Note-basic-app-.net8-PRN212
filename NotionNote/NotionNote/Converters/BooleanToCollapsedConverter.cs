using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NotionNote.Converters
{
    public class BooleanToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This is just a placeholder - the actual text is set via Style triggers
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

