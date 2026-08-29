
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace RogueEssence.Dev.Converters
{
    public class ObjectToColorKeyConverter : IMultiValueConverter
    {
        public object? Convert(IList<object> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2)
                throw new Exception("Expected at least 2 values: object, mandatoryKey, [optionalKey]");

            var value = values[0];
            var mandatoryKey = values[1] as string;
            var optionalKey = values.Count > 2 ? values[2] as string : null;

            if (mandatoryKey == null)
                throw new Exception("Mandatory key must be a string");

            if (value == null)
            {
                if (Application.Current?.TryGetResource(mandatoryKey,
                        Application.Current.ActualThemeVariant, out var brush) == true)
                {
                    return brush;
                }

            }

            if (optionalKey == null) return AvaloniaProperty.UnsetValue;


            if (Application.Current?.TryGetResource(optionalKey,
                    Application.Current.ActualThemeVariant, out var brushWhenNotNull) == true)
            {
                return brushWhenNotNull;
            }


            return Brushes.Transparent;
            throw new Exception($"No brush found!");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}