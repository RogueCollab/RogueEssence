using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace RogueEssence.Dev.Converters
{
    public class ListNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList list = (IList)value;
            return list.Count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
