using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace RogueEssence.Dev.Converters
{
    public class TilesetScaledSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count > 1 && values[0] is string tileset && !String.IsNullOrEmpty(tileset) && values[1] is int zoomPercent)
            {
                Bitmap bitmap = DevDataManager.GetTileset(tileset);
                if (bitmap == null)
                    return 0.0;
                bool useHeight = Boolean.Parse((string)parameter);
                int size = useHeight ? bitmap.PixelSize.Height : bitmap.PixelSize.Width;
                return size * zoomPercent / 100.0;
            }
            return 0.0;
        }
    }
}
