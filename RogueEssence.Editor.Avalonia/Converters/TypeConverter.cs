using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RogueEssence.Dev.Converters
{
    public class TypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = (string)value;
            return DevDataManager.GetTypeIcon(type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}