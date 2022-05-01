using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Resources.NetStandard;


namespace RogueEssence
{
    public class LanguageSetting
    {
        public string Name;
        public List<string> Fallbacks;

        public LanguageSetting(string name, List<string> fallbacks)
        {
            Name = name;
            Fallbacks = new List<string>();
            Fallbacks.AddRange(fallbacks);
        }
    }
    public static class Text
    {
        public const string DIVIDER_STR = "\n";
        public static List<Dictionary<string, string>> Strings;
        public static List<Dictionary<string, string>> StringsEx;
        public static CultureInfo Culture;
        public static string[] SupportedLangs;
        public static Dictionary<string, LanguageSetting> LangNames;

        public static Regex MsgTags = new Regex(@"(?<pause>\[pause=(?<pauseval>\d+)\])" +
                                                @"|(?<colorstart>\[color=#(?<colorval>[0-9a-f]{6})\])|(?<colorend>\[color\])" +
                                                @"|(?<boxbreak>\[br\])" +
                                                @"|(?<scrollbreak>\[scroll\])" +
                                                @"(?<script>\[script=(?<scriptval>\d+)\])",
                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Strings = new List<Dictionary<string, string>>();
            StringsEx = new List<Dictionary<string, string>>();

            List<string> codes = new List<string>();
            Dictionary<string, LanguageSetting> translations = new Dictionary<string, LanguageSetting>();
            try
            {
                foreach (string path in PathMod.FallforthPaths("Strings/Languages.xml"))
                {
                    if (File.Exists(path))
                    {
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(path);
                        foreach (XmlNode xnode in xmldoc.DocumentElement.ChildNodes)
                        {
                            if (xnode.Name == "data")
                            {
                                string value = null;
                                string name = null;
                                var atname = xnode.Attributes["name"];
                                if (atname != null)
                                    name = atname.Value;

                                //Get value
                                XmlNode valnode = xnode.SelectSingleNode("value");
                                if (valnode != null)
                                    value = valnode.InnerText;

                                List<string> fallbacks = new List<string>();
                                foreach (XmlNode fallbacknode in xnode.SelectNodes("fallback"))
                                    fallbacks.Add(fallbacknode.InnerText);

                                if (!codes.Contains(name))
                                    codes.Add(name);
                                translations[name] = new LanguageSetting(value, fallbacks);
                            }
                        }
                    }
                }
                SupportedLangs = codes.ToArray();
                LangNames = translations;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                SupportedLangs = new string[1] { "en" };
                LangNames["en"] = new LanguageSetting("English", new List<string>());
            }
        }

        public static string ToName(this string lang)
        {
            return LangNames[lang].Name;
        }

        public static Dictionary<string, string> LoadStringResx(string path)
        {
            try
            {
                Dictionary<string, string> translations = new Dictionary<string, string>();
                if (File.Exists(path))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);
                    foreach (XmlNode xnode in xmldoc.DocumentElement.ChildNodes)
                    {
                        if (xnode.Name == "data")
                        {
                            string value = null;
                            string name = null;
                            var atname = xnode.Attributes["name"];
                            if (atname != null)
                                name = atname.Value;

                            //Get value
                            XmlNode valnode = xnode.SelectSingleNode("value");
                            if (valnode != null)
                                value = valnode.InnerText;

                            if (value != null && name != null)
                                translations[name] = value;
                        }
                    }
                }
                return translations;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return new Dictionary<string, string>();
            }
        }

        public static Dictionary<string, (string val, string comment)> LoadDevStringResx(string path)
        {
            try
            {
                Dictionary<string, (string val, string comment)> translations = new Dictionary<string, (string val, string comment)>();
                if (File.Exists(path))
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);
                    foreach (XmlNode xnode in xmldoc.DocumentElement.ChildNodes)
                    {
                        if (xnode.Name == "data")
                        {
                            string value = null;
                            string name = null;
                            string comment = "";
                            var atname = xnode.Attributes["name"];
                            if (atname != null)
                                name = atname.Value;

                            //Get value
                            XmlNode valnode = xnode.SelectSingleNode("value");
                            if (valnode != null)
                                value = valnode.InnerText;

                            //Get comment
                            XmlNode comnode = xnode.SelectSingleNode("comment");
                            if (comnode != null)
                                comment = comnode.InnerText;

                            if (value != null && name != null)
                                translations[name] = (value, comment);
                        }
                    }
                }
                return translations;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return new Dictionary<string, (string, string)>();
            }
        }

        public static void SaveStringResx(string path, Dictionary<string, (string val, string comment)> stringDict)
        {
            using (ResXResourceWriter resx = new ResXResourceWriter(path))
            {
                foreach (string key in stringDict.Keys)
                    resx.AddResource(new ResXDataNode(key, stringDict[key].val) { Comment = stringDict[key].comment });

                resx.Generate();
                resx.Close();
            }
        }

        public static string FormatKey(string key, params object[] args)
        {
            try
            {
                //take a resource instead of a string, and return the localized string for it
                string text = "";
                for (int ii = 0; ii < Strings.Count; ii++)
                {
                    if (Text.Strings[ii].TryGetValue(key, out text))
                        return String.Format(Regex.Unescape(text), args);
                }
                throw new KeyNotFoundException(String.Format("Could not find value for {0}", key));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return key;
        }
        public static string ToLocal(this Enum value, string extra)
        {
            string key = "_ENUM_" + value.GetType().Name + "_" + value;
            if (extra != null)
                key += "_" + extra;

            string text = "";
            for (int ii = 0; ii < Strings.Count; ii++)
            {
                if (Text.Strings[ii].TryGetValue(key, out text))
                    break;
            }

            if (!String.IsNullOrEmpty(text))
                return Regex.Unescape(text);
            return value.ToString();
        }
        public static string ToLocal(this Enum value)
        {
            return value.ToLocal(null);
        }

        public static string ToEscaped(this string str)
        {
            StringBuilder builder = new StringBuilder();
            for (int ii = 0; ii < str.Length; ii++)
            {
                if (str[ii] > 0xFF)
                    builder.Append("\\u" + ((int)str[ii]).ToString("X4"));
                else
                    builder.Append(str[ii]);

            }
            return builder.ToString();
        }

        public static string BuildList(string[] input)
        {
            StringBuilder totalString = new StringBuilder();
            for (int ii = 0; ii < input.Length; ii++)
            {
                if (ii > 0)
                {
                    if (input.Length > 2)
                        totalString.Append(", ");
                    else if (ii == input.Length - 1)
                        totalString.Append(Text.FormatKey("ADD_END") + " ");
                }
                totalString.Append(input[ii]);
            }
            return totalString.ToString();
        }

        public static void SetCultureCode(string code)
        {
            Culture = new CultureInfo(code);

            loadCulture(Strings, code, "strings");

            loadCulture(StringsEx, code, "stringsEx");
        }

        private static void loadCulture(List<Dictionary<string, string>> strings, string code, string fileName)
        {
            strings.Clear();
            //order of string fallbacks:
            //first go through all mods of the original language
            foreach(string path in PathMod.FallbackPaths("Strings/" + fileName + "." + code + ".resx"))
                strings.Add(LoadStringResx(path));

            //then go through all mods of the official fallbacks
            if (LangNames.ContainsKey(code))
            {
                foreach (string fallback in LangNames[code].Fallbacks)
                {
                    foreach (string path in PathMod.FallbackPaths("Strings/" + fileName + "." + fallback + ".resx"))
                        strings.Add(LoadStringResx(path));
                }
            }
            //then go through all mods of the default language
            foreach (string path in PathMod.FallbackPaths("Strings/" + fileName + ".resx"))
                strings.Add(LoadStringResx(path));
        }
    }



    [Serializable]
    public struct StringKey
    {
        public string Key;

        public StringKey(string key)
        {
            Key = key;
        }

        public string ToLocal()
        {
            try
            {
                string val = "";
                for (int ii = 0; ii < Text.StringsEx.Count; ii++)
                {
                    if (Text.StringsEx[ii].TryGetValue(Key, out val))
                        return Regex.Unescape(val);
                }
                throw new KeyNotFoundException(String.Format("Could not find value for {0}", Key));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return Key;
        }

        public override string ToString()
        {
            if (Key != null)
                return Key;
            return "";
        }

        public bool IsValid()
        {
            return !String.IsNullOrWhiteSpace(Key);
        }
    }


    [Serializable]
    public abstract class LocalFormat
    {
        public StringKey Key;

        public LocalFormat() { Key = new StringKey(""); }
        public LocalFormat(LocalFormat other) { Key = other.Key; }
        public abstract LocalFormat Clone();

        public abstract string FormatLocal();
    }

    [Serializable]
    public class LocalFormatEnum<T> : LocalFormat where T : Enum
    {
        public List<T> Enums;

        public LocalFormatEnum() { Enums = new List<T>(); }
        public LocalFormatEnum(LocalFormatEnum<T> other) : base(other)
        {
            Enums = new List<T>();
            Enums.AddRange(other.Enums);
        }
        public override LocalFormat Clone() { return new LocalFormatEnum<T>(this); }

        public override string FormatLocal()
        {
            List<string> enumStrings = new List<string>();
            foreach (T t in Enums)
                enumStrings.Add(t.ToLocal());
            return String.Format(Key.ToLocal(), enumStrings.ToArray());
        }
    }

    [Serializable]
    public class LocalFormatSimple : LocalFormat
    {
        public List<StringKey> Args;

        public LocalFormatSimple() { Args = new List<StringKey>(); }
        public LocalFormatSimple(string keyString, params string[] args)
        {
            Key = new StringKey(keyString);
            Args = new List<StringKey>();
            foreach (string arg in args)
                Args.Add(new StringKey(arg));
        }
        public LocalFormatSimple(StringKey key, params StringKey[] args)
        {
            Key = key;
            Args = new List<StringKey>();
            foreach (StringKey arg in args)
                Args.Add(arg);
        }
        public LocalFormatSimple(LocalFormatSimple other) : base(other)
        {
            Args = new List<StringKey>();
            Args.AddRange(other.Args);
        }
        public override LocalFormat Clone() { return new LocalFormatSimple(this); }

        public override string FormatLocal()
        {
            object[] args = new object[Args.Count];
            for (int ii = 0; ii < args.Length; ii++)
                args[ii] = Args[ii].ToLocal();
            return String.Format(Key.ToLocal(), args);
        }
    }

    [Serializable]
    public class LocalText
    {
        public string DefaultText;

        public Dictionary<string, string> LocalTexts;

        public LocalText()
        {
            DefaultText = "";
            LocalTexts = new Dictionary<string, string>();
        }

        public LocalText(string defaultText)
        {
            DefaultText = defaultText;
            LocalTexts = new Dictionary<string, string>();
        }
        public LocalText(LocalText other)
        {
            DefaultText = other.DefaultText;
            LocalTexts = new Dictionary<string, string>();
            foreach (string key in other.LocalTexts.Keys)
                LocalTexts.Add(key, other.LocalTexts[key]);
        }
        public LocalText(LocalText other, string[] args)
        {
            DefaultText = String.Format(other.DefaultText, args);
            LocalTexts = new Dictionary<string, string>();
            foreach (string key in other.LocalTexts.Keys)
                LocalTexts.Add(key, String.Format(other.LocalTexts[key], args));
        }
        public LocalText(LocalText other, LocalText[] args)
        {
            string[] defaultArgs = new string[args.Length];
            for (int ii = 0; ii < args.Length; ii++)
                defaultArgs[ii] = args[ii].DefaultText;
            DefaultText = String.Format(other.DefaultText, defaultArgs);
            LocalTexts = new Dictionary<string, string>();
            foreach (string key in other.LocalTexts.Keys)
            {
                string[] localArgs = new string[args.Length];
                for (int ii = 0; ii < args.Length; ii++)
                {
                    if (args[ii].LocalTexts.ContainsKey(key))
                        localArgs[ii] = args[ii].LocalTexts[key];
                    else//if there is no translation for this string argument, fall back on default text.
                        localArgs[ii] = args[ii].DefaultText;
                }
                LocalTexts.Add(key, String.Format(other.LocalTexts[key], localArgs));
            }
        }
        public static LocalText FormatLocalText(LocalText format, params string[] args)
        {
            return new LocalText(format, args);
        }
        public static LocalText FormatLocalText(LocalText format, params LocalText[] args)
        {
            return new LocalText(format, args);
        }

        public string ToLocal()
        {
            string text;
            if (LocalTexts.TryGetValue(Text.Culture.Name.ToLower(), out text))
                return Regex.Unescape(text);

            if (Text.LangNames.ContainsKey(Text.Culture.Name))
            {
                foreach (string fallback in Text.LangNames[Text.Culture.Name].Fallbacks)
                {
                    if (LocalTexts.TryGetValue(fallback, out text))
                        return Regex.Unescape(text);
                }
            }

            return Regex.Unescape(DefaultText);
        }


        public override string ToString()
        {
            return DefaultText;
        }
    }

}
