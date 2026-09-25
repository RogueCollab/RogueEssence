using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Converters
{
    public class TileToThicknessConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is TileFrame tileFrame && values[1] is int tileSize)
            {
                int zoomPercent = values.Count > 2 && values[2] is int zoom ? zoom : 100;
                double scaledTileSize = tileSize * zoomPercent / 100.0;
                return new Thickness(tileFrame.TexLoc.X * scaledTileSize, tileFrame.TexLoc.Y * scaledTileSize, 0, 0);
            }
            return new Thickness();
        }
    }
}
