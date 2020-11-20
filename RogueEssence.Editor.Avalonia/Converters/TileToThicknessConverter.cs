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
                return new Thickness(tileFrame.TexLoc.X * tileSize, tileFrame.TexLoc.Y * tileSize, 0, 0);
            return new Thickness();
        }
    }
}
