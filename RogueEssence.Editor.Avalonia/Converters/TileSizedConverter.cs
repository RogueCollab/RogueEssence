using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Converters
{
    public class TileSizedConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is Loc tileXY && values[1] is int tileSize)
            {
                bool useY = Boolean.Parse((string)parameter);
                int diff = useY ? tileXY.Y : tileXY.X;
                return diff * tileSize;
            }
            return 0;
        }
    }
}
