using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Converters
{
    public class FrameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TileFrame frame)
            {
                if (frame == TileFrame.Empty)
                    return "[EMPTY]";
                long tilePos = GraphicsManager.TileIndex.GetPosition(frame.Sheet, frame.TexLoc);
                if (tilePos > 0)
                    return String.Format("{0}: X{1} Y{2}", frame.Sheet, frame.TexLoc.X, frame.TexLoc.Y);
                else
                    return String.Format("[X] {0}: X{1} Y{2}", frame.Sheet, frame.TexLoc.X, frame.TexLoc.Y);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
