using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Converters
{
    public class OXConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool tf = (bool)value;
            return tf ? DevTileManager.IconO : DevTileManager.IconX;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
