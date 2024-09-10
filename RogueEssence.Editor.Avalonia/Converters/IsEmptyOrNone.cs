using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RogueEssence.Dev.Converters
{
    public class IsNoneOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = (string) value;
            s = s.ToLower();
            
            bool res = s == "none" || s == "";
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    
    public class IsNotNoneOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = (string)value;
            s = s.ToLower();
            bool res = !(s == "none" || s == "");
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}