using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using RogueEssence.Script;

namespace RogueEssence.Dev.Converters
{
    public class MapScriptPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string file = (string)value;
            if (file == "")
                return "Script Data [Map not yet saved]";

            string mapscriptdir = LuaEngine.MakeGroundMapScriptPath(Path.GetFileNameWithoutExtension(file), "");
            return String.Format("Script Data [{0}]", mapscriptdir);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
