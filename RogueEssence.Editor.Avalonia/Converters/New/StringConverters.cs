
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Styling;

namespace RogueEssence.Dev.Converters
{
    public class ToLocaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Models.Locale.Supported.Find(x => x.Key == value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Models.Locale)?.Key;
        }
    }

    public static class StringConverters
    {
        public static readonly ToLocaleConverter ToLocale = new ToLocaleConverter();

        public class ToThemeConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var theme = (string)value;
                if (string.IsNullOrEmpty(theme))
                    return ThemeVariant.Default;

                if (theme.Equals("Light", StringComparison.OrdinalIgnoreCase))
                    return ThemeVariant.Light;

                if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
                    return ThemeVariant.Dark;

                return ThemeVariant.Default;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (value as ThemeVariant)?.Key;
            }
        }

        public static readonly ToThemeConverter ToTheme = new ToThemeConverter();
    }
}