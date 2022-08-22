using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev.Converters
{
    public class DataEntryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string idx)
            {
                DataManager.DataType dataType = (DataManager.DataType)Int32.Parse((string)parameter);
                EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataType];
                if (nameIndex.ContainsKey(idx))
                    return nameIndex.Get(idx).Name.ToLocal();
                return "**EMPTY**";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
