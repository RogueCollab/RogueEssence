using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Converters
{
    public class MultiSelectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Loc size = (Loc)value;
            //if (size != Loc.One)
            return String.Format("Multi-Select: {0}x{1}", size.X, size.Y);
            //return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
