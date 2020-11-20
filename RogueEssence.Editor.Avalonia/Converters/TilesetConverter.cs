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
    public class TilesetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tileset = (string)value;
            if (String.IsNullOrEmpty(tileset))
                return null;
            return DevTileManager.Instance.GetTileset(tileset);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
