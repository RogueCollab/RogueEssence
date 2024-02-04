using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Resources.NetStandard;
using RogueElements;
using RogueEssence.Data;

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
        public static Dictionary<string, string> Strings;
        public static Dictionary<string, string> StringsEx;
        public static CultureInfo Culture;
        public static string[] SupportedLangs;
        public static Dictionary<string, LanguageSetting> LangNames;

        public static Regex MsgTags = new Regex(@"(?<pause>\[pause=(?<pauseval>\d+)\])" +
                                                @"|(?<sound>\[sound=(?<soundval>[A-Za-z\/0-9\-_]*),?(?<speaktime>\d*)?\])" +
                                                @"|(?<colorstart>\[color=#(?<colorval>[0-9a-f]{6})\])|(?<colorend>\[color\])" +
                                                @"|(?<boxbreak>\[br\])" +
                                                @"|(?<scrollbreak>\[scroll\])" +
                                                @"|(?<script>\[script=(?<scriptval>\d+)\])" +
                                                @"|(?<speed>\[speed=(?<speedval>[+-]?\d+\.?\d*)\])" + 
                                                @"|(?<emote>\[emote=(?<emoteval>[a-zA-Z0-9\-]*)\])",
                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex GrammarTags = new Regex(@"(?<a_an>\[a/an\]\W+(?<a_anval>\w))" + //en
                                                @"|(?<el_la>\[el/la\]\W+?(?<el_lasex>\[male\]|\[female\]|\[neutral\])?\w)" + //es
                                                @"|(?<der_die_das>\[der/die/das\]\W+?(?<der_die_dassex>\[male\]|\[female\]|\[neutral\])?\w)" + //de
                                                @"|(?<ein_eine_einen>\[ein/eine/einen\]\W+?(?<ein_eine_einensex>\[male\]|\[female\]|\[neutral\])?\w)" + //de
                                                @"|(?<ein_eine_ein>\[ein/eine/ein\]\W+?(?<ein_eine_einsex>\[male\]|\[female\]|\[neutral\])?\w)" + //de
                                                @"|(?<il_la>\[il/la\]\W+?(?<il_lasex>\[male\]|\[female\]|\[neutral\])?(?<il_laval>\w\w?))" + //it
                                                @"|(?<i_le>\[i/le\]\W+?(?<i_lesex>\[male\]|\[female\]|\[neutral\])?(?<i_leval>\w\w?))" + //it
                                                @"|(?<uno_una>\[uno/una\]\W+?(?<uno_unasex>\[male\]|\[female\]|\[neutral\])?(?<uno_unaval>\w))" + //it
                                                @"|(?<eun_neun>(?<eun_neunval>\w)\[은/는\])" + //ko
                                                @"|(?<eul_leul>(?<eul_leulval>\w)\[을/를\])" + //ko
                                                @"|(?<i_ga>(?<i_gaval>\w)\[이/가\])" + //ko
                                                @"|(?<wa_gwa>(?<wa_gwaval>\w)\[와/과\])" + //ko
                                                @"|(?<eu_lo>(?<eu_loval>\w)\[으/로\])" + //ko
                                                @"|(?<i_lamyeon>(?<i_lamyeonval>\w)\[이/라면\])", //ko
                                                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Strings = new Dictionary<string, string>();
            StringsEx = new Dictionary<string, string>();

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

        public static string FormatGrammar(string input, params object[] args)
        {
            string output = String.Format(input, args);

            List<(int idx, string replace)> reInserts = new List<(int, string)>();
            int lag = 0;
            MatchCollection tagMatches = MsgTags.Matches(output);
            foreach (Match match in tagMatches)
            {
                reInserts.Add((match.Index - lag, output.Substring(match.Index - lag, match.Length)));
                output = output.Remove(match.Index - lag, match.Length);
                lag += match.Length;
            }

            List<(int idx, int length, string replace)> replacements = new List<(int, int, string)>();
            MatchCollection matches = GrammarTags.Matches(output);
            foreach (Match match in matches)
            {
                foreach (string key in match.Groups.Keys)
                {
                    if (!match.Groups[key].Success)
                        continue;
                    switch (key)
                    {
                        case "a_an":
                            {
                                string vowelcheck = match.Groups["a_anval"].Value;

                                if (Regex.IsMatch(vowelcheck, "^[aeiou]", RegexOptions.IgnoreCase))
                                    replacements.Add((match.Index, "[a/an]".Length, "an"));
                                else
                                    replacements.Add((match.Index, "[a/an]".Length, "a"));
                            }
                            break;
                        case "el_la":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["el_lasex"].Value, Gender.Male);

                                if (gendercheck == Gender.Male)
                                    replacements.Add((match.Index, "[el/la]".Length, "el"));
                                else
                                    replacements.Add((match.Index, "[el/la]".Length, "la"));

                                if (match.Groups["el_lasex"].Success)
                                    replacements.Add((match.Groups["el_lasex"].Index, match.Groups["el_lasex"].Value.Length, ""));
                            }
                            break;
                        case "der_die_das":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["der_die_dassex"].Value, Gender.Male);

                                if (gendercheck == Gender.Male)
                                    replacements.Add((match.Index, "[der/die/das]".Length, "der"));
                                else if (gendercheck == Gender.Female)
                                    replacements.Add((match.Index, "[der/die/das]".Length, "die"));
                                else
                                    replacements.Add((match.Index, "[der/die/das]".Length, "das"));

                                if (match.Groups["der_die_dassex"].Success)
                                    replacements.Add((match.Groups["der_die_dassex"].Index, match.Groups["der_die_dassex"].Value.Length, ""));
                            }
                            break;
                        case "ein_eine_einen":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["ein_eine_einensex"].Value, Gender.Male);

                                if (gendercheck == Gender.Male)
                                    replacements.Add((match.Index, "[ein/eine/einen]".Length, "ein"));
                                else if (gendercheck == Gender.Female)
                                    replacements.Add((match.Index, "[ein/eine/einen]".Length, "eine"));
                                else
                                    replacements.Add((match.Index, "[ein/eine/einen]".Length, "einen"));

                                if (match.Groups["ein_eine_einensex"].Success)
                                    replacements.Add((match.Groups["ein_eine_einensex"].Index, match.Groups["ein_eine_einensex"].Value.Length, ""));
                            }
                            break;
                        case "ein_eine_ein":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["ein_eine_einsex"].Value, Gender.Male);

                                if (gendercheck == Gender.Female)
                                    replacements.Add((match.Index, "[ein/eine/ein]".Length, "eine"));
                                else
                                    replacements.Add((match.Index, "[ein/eine/ein]".Length, "ein"));

                                if (match.Groups["ein_eine_einsex"].Success)
                                    replacements.Add((match.Groups["ein_eine_einsex"].Index, match.Groups["ein_eine_einsex"].Value.Length, ""));
                            }
                            break;
                        case "il_la":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["il_lasex"].Value, Gender.Male);
                                string vowelcheck = match.Groups["il_laval"].Value;
                                string postMatch = "";

                                if (Regex.IsMatch(vowelcheck, "^([aeou]|i[bcdfghjklmnpqrstvwxyz])", RegexOptions.IgnoreCase))
                                {
                                    replacements.Add((match.Index, "[il/la]".Length, ""));
                                    postMatch = "l'";
                                }
                                else
                                {
                                    if (gendercheck == Gender.Male)
                                    {
                                        if (Regex.IsMatch(vowelcheck, "^(x|y|z|s[bcdfghjklmnpqrstvwxyz]|gn|ps|pn|i[aeiou])", RegexOptions.IgnoreCase))
                                            replacements.Add((match.Index, "[il/la]".Length, "lo"));
                                        else
                                            replacements.Add((match.Index, "[il/la]".Length, "il"));
                                    }
                                    else
                                    {
                                        replacements.Add((match.Index, "[il/la]".Length, "la"));
                                    }
                                }

                                if (match.Groups["il_lasex"].Success)
                                    replacements.Add((match.Groups["il_lasex"].Index, match.Groups["il_lasex"].Value.Length, ""));

                                if (!String.IsNullOrEmpty(postMatch))
                                    replacements.Add((match.Groups["il_laval"].Index, 0, postMatch));
                            }
                            break;
                        case "i_le":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["i_lesex"].Value, Gender.Male);
                                string vowelcheck = match.Groups["i_leval"].Value;

                                if (gendercheck == Gender.Male)
                                {
                                    if (Regex.IsMatch(vowelcheck, "^(x|y|z|s[bcdfghjklmnpqrstvwxyz]|gn|ps|pn|[aeiou])", RegexOptions.IgnoreCase))
                                        replacements.Add((match.Index, "[i/le]".Length, "gli"));
                                    else
                                        replacements.Add((match.Index, "[i/le]".Length, "i"));
                                }
                                else
                                    replacements.Add((match.Index, "[i/le]".Length, "le"));

                                if (match.Groups["i_lesex"].Success)
                                    replacements.Add((match.Groups["i_lesex"].Index, match.Groups["i_lesex"].Value.Length, ""));
                            }
                            break;
                        case "uno_una":
                            {
                                Gender gendercheck = extractGenderTag(match.Groups["uno_unasex"].Value, Gender.Male);
                                string vowelcheck = match.Groups["uno_unaval"].Value;
                                string postMatch = "";

                                if (gendercheck == Gender.Male)
                                {
                                    if (Regex.IsMatch(vowelcheck, "^[aeiou]", RegexOptions.IgnoreCase))
                                        replacements.Add((match.Index, "[uno/una]".Length, "un"));
                                    else
                                        replacements.Add((match.Index, "[uno/una]".Length, "uno"));
                                }
                                else
                                {
                                    if (Regex.IsMatch(vowelcheck, "^[aeiou]", RegexOptions.IgnoreCase))
                                    {
                                        replacements.Add((match.Index, "[uno/una]".Length, ""));
                                        postMatch = "un'";
                                    }
                                    else
                                        replacements.Add((match.Index, "[uno/una]".Length, "una"));
                                }

                                if (match.Groups["uno_unasex"].Success)
                                    replacements.Add((match.Groups["uno_unasex"].Index, match.Groups["uno_unasex"].Value.Length, ""));

                                if (!String.IsNullOrEmpty(postMatch))
                                    replacements.Add((match.Groups["uno_unaval"].Index, 0, postMatch));
                            }
                            break;
                        case "eun_neun":
                            {
                                string vowelcheck = match.Groups["eun_neunval"].Value;
                                char vowelchar = vowelcheck[0];
                                if ((int)(vowelchar - '가') % 28 == 0)
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "는"));
                                else
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "은"));
                            }
                            break;
                        case "eul_leul":
                            {
                                string vowelcheck = match.Groups["eul_leulval"].Value;
                                char vowelchar = vowelcheck[0];
                                if ((int)(vowelchar - '가') % 28 == 0)
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "를"));
                                else
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "을"));
                            }
                            break;
                        case "i_ga":
                            {
                                string vowelcheck = match.Groups["i_gaval"].Value;
                                char vowelchar = vowelcheck[0];
                                if ((int)(vowelchar - '가') % 28 == 0)
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "가"));
                                else
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "이"));
                            }
                            break;
                        case "wa_gwa":
                            {
                                string vowelcheck = match.Groups["wa_gwaval"].Value;
                                char vowelchar = vowelcheck[0];
                                if ((int)(vowelchar - '가') % 28 == 0)
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "과"));
                                else
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "와"));
                            }
                            break;
                        case "eu_lo":
                            {
                                string vowelcheck = match.Groups["eu_loval"].Value;
                                char vowelchar = vowelcheck[0];
                                if ((int)(vowelchar - '가') % 28 == 0)
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "로"));
                                else
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "으"));
                            }
                            break;
                        case "i_lamyeon":
                            {
                                string vowelcheck = match.Groups["i_lamyeonval"].Value;
                                char vowelchar = vowelcheck[0];
                                if ((int)(vowelchar - '가') % 28 == 0)
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "라면"));
                                else
                                    replacements.Add((match.Index + vowelcheck.Length, match.Length - vowelcheck.Length, "이"));
                            }
                            break;
                    }
                }
            }

            int reIdx = reInserts.Count - 1;
            for (int ii = replacements.Count - 1; ii >= 0; ii--)
            {
                while (reIdx > -1)
                {
                    if (reInserts[reIdx].Item1 < replacements[ii].idx + replacements[ii].length)
                        break;

                    output = output.Insert(reInserts[reIdx].idx, reInserts[reIdx].replace);
                    reIdx--;
                }

                output = output.Remove(replacements[ii].idx, replacements[ii].length);
                output = output.Insert(replacements[ii].idx, replacements[ii].replace);
            }

            while (reIdx > -1)
            {
                output = output.Insert(reInserts[reIdx].Item1, reInserts[reIdx].replace);
                reIdx--;
            }

            return output;
        }

        private static Gender extractGenderTag(string genderStr, Gender defaultGender)
        {
            switch (genderStr.ToLower())
            {
                case "[male]":
                    return Gender.Male;
                case "[female]":
                    return Gender.Female;
                case "[neutral]":
                    return Gender.Genderless;
            }

            return defaultGender;
        }

        public static string FormatKey(string key, params object[] args)
        {
            try
            {
                //take a resource instead of a string, and return the localized string for it
                string text;
                if (Text.Strings.TryGetValue(key, out text))
                    return FormatGrammar(Regex.Unescape(text), args);
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

            string text;
            if (Text.Strings.TryGetValue(key, out text))
            {
                if (!String.IsNullOrEmpty(text))
                    return Regex.Unescape(text);
            }
    
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
                    if (ii == input.Length - 1)
                        totalString.Append(Text.FormatKey("ADD_END"));
                    else
                        totalString.Append(Text.FormatKey("ADD_SEPARATOR"));
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

        private static void loadCulture(Dictionary<string, string> strings, string code, string fileName)
        {
            strings.Clear();
            //order of string fallbacks:
            //first go through all mods of the original language
            foreach (string path in PathMod.FallbackPaths("Strings/" + fileName + "." + code + ".resx"))
            {
                Dictionary<string, string> dict = LoadStringResx(path);
                foreach (string key in dict.Keys)
                {
                    if (!strings.ContainsKey(key))
                        strings.Add(key, dict[key]);
                }
            }

            //then go through all mods of the official fallbacks
            if (LangNames.ContainsKey(code))
            {
                foreach (string fallback in LangNames[code].Fallbacks)
                {
                    foreach (string path in PathMod.FallbackPaths("Strings/" + fileName + "." + fallback + ".resx"))
                    {
                        Dictionary<string, string> dict = LoadStringResx(path);
                        foreach (string key in dict.Keys)
                        {
                            if (!strings.ContainsKey(key))
                                strings.Add(key, dict[key]);
                        }
                    }
                }
            }
            //then go through all mods of the default language
            foreach (string path in PathMod.FallbackPaths("Strings/" + fileName + ".resx"))
            {
                Dictionary<string, string> dict = LoadStringResx(path);
                foreach (string key in dict.Keys)
                {
                    if (!strings.ContainsKey(key))
                        strings.Add(key, dict[key]);
                }
            }
        }

        public static string GetLanguagedPath(string basePath, string cultureCode)
        {
            if (String.IsNullOrEmpty(cultureCode))
                return basePath;

            string dir = Path.GetDirectoryName(basePath);
            string noExt = Path.GetFileNameWithoutExtension(basePath);
            string ext = Path.GetExtension(basePath);
            return Path.Join(dir, noExt + "." + cultureCode + ext);
        }

        public static string ModLangPath(string basePath)
        {
            string cultureCode = Culture.Name.ToLower();
            string langPath = GetLanguagedPath(basePath, cultureCode);
            if (File.Exists(langPath) || Directory.Exists(langPath))
                return langPath;
            foreach (string fallback in LangNames[cultureCode].Fallbacks)
            {
                langPath = GetLanguagedPath(basePath, cultureCode);
                if (File.Exists(langPath) || Directory.Exists(langPath))
                    return langPath;
            }

            return basePath;
        }

        public static string Sanitize(string input)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = input.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            string result = Regex.Replace(sbReturn.ToString(), "[':.]", "");
            result = Regex.Replace(result, "\\W", "_");
            return result;
        }

        public static string GetNonConflictingName(string inputStr, Func<string, bool> getConflict)
        {
            string prefix = inputStr;
            int origIndex;
            int lastUnderscore = inputStr.LastIndexOf('_');
            if (lastUnderscore > -1)
            {
                string substr = inputStr.Substring(lastUnderscore + 1);
                if (int.TryParse(substr, out origIndex))
                    prefix = inputStr.Substring(0, lastUnderscore);
            }

            if (!getConflict(inputStr))
                return inputStr;

            int copy_index = 1;
            while (copy_index < Int32.MaxValue)
            {
                if (!getConflict(prefix + "_" + copy_index.ToString()))
                    return prefix + "_" + copy_index.ToString();

                copy_index++;
            }

            return null;
        }

        public static string GetMemberTitle(string name)
        {
            StringBuilder separatedName = new StringBuilder();
            for (int ii = 0; ii < name.Length; ii++)
            {
                if (ii > 0)
                {
                    bool space = false;
                    if (char.IsDigit(name[ii]) && char.IsLetter(name[ii - 1]))
                        space = true;
                    if (char.IsUpper(name[ii]) && char.IsLower(name[ii - 1]) || char.IsUpper(name[ii]) && char.IsDigit(name[ii - 1]))
                        space = true;
                    if (char.IsUpper(name[ii]) && char.IsUpper(name[ii - 1]) && ii < name.Length - 1 && char.IsLower(name[ii + 1]))
                        space = true;
                    if (space)
                        separatedName.Append(' ');
                }
                separatedName.Append(name[ii]);
            }
            return separatedName.ToString();
        }

        public static int DeterministicHash(string str)
        {
            //TODO: we don't know if this is consistent between 32bit and 64bit machines...
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
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
                string val;
                if (Text.StringsEx.TryGetValue(Key, out val))
                    return Regex.Unescape(val);
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
            return Text.FormatGrammar(Key.ToLocal(), enumStrings.ToArray());
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
            return Text.FormatGrammar(Key.ToLocal(), args);
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
            DefaultText = Text.FormatGrammar(other.DefaultText, args);
            LocalTexts = new Dictionary<string, string>();
            foreach (string key in other.LocalTexts.Keys)
                LocalTexts.Add(key, Text.FormatGrammar(other.LocalTexts[key], args));
        }
        public LocalText(LocalText other, LocalText[] args)
        {
            string[] defaultArgs = new string[args.Length];
            for (int ii = 0; ii < args.Length; ii++)
                defaultArgs[ii] = args[ii].DefaultText;
            DefaultText = Text.FormatGrammar(other.DefaultText, defaultArgs);
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
                LocalTexts.Add(key, Text.FormatGrammar(other.LocalTexts[key], localArgs));
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
