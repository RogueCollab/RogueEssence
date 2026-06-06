using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using RogueEssence.Script;

namespace RogueEssence.Dev.Converters
{
    public class MathAdd : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValue = (int)value;
            var paramStr = (string)parameter;
            var isParseable = int.TryParse(paramStr, out var offset);
    
            if (!isParseable) return value;
    
            return intValue + offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}