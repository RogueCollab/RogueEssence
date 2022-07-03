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
            if (value is int idx)
            {
                DataManager.DataType dataType = (DataManager.DataType)Int32.Parse((string)parameter);
                EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataType];
                //TODO: String Assets
                if (idx >= 0 && idx < nameIndex.Count)
                    return nameIndex.Entries[idx.ToString()].Name.ToLocal();
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
