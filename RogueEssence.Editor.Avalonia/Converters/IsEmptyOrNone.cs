using System;
using System.Globalization;
using Avalonia.Data.Converters;
using RogueEssence.Data;

namespace RogueEssence.Dev.Converters
{
    public class IsNoneOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataManager.DataType dataType = (DataManager.DataType)Int32.Parse((string)parameter);
            string s = (string) value;
            s = s.ToLower();
            
            bool res = (String.IsNullOrEmpty(s) || s == DataManager.Instance.GetDefaultData(dataType));
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
            DataManager.DataType dataType = (DataManager.DataType)Int32.Parse((string)parameter);
            string s = (string)value;
            s = s.ToLower();
            bool res = !(String.IsNullOrEmpty(s) || s == DataManager.Instance.GetDefaultData(dataType));
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}