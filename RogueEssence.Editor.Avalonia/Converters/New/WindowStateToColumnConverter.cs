using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace RogueEssence.Dev.Converters
{
    public class WindowStateToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowState ws)
            {
                if (ws == WindowState.FullScreen || !OperatingSystem.IsMacOS())
                    return 0;
            }

            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}