using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NLua;
using System.IO;

namespace RogueEssence.Script
{
    /// <summary>
    /// Class for handling loading localized string via xml, because lua xml libs sucks
    /// </summary>
    public class ScriptStrings : ILuaEngineComponent
    {
        public const string STRINGS_FILE_NAME = "strings";

        public LuaTable MakePackageStringTable(string packagefilepath)
        {
            try
            {
                Dictionary<string, string> xmlDict = new Dictionary<string, string>();

                string code = LocaleCode();
                //order of string fallbacks:
                //first go through all mods of the original language
                foreach (ModHeader mod in PathMod.FallbackMods(LuaEngine.SCRIPT_PATH))
                {
                    string modulePath = PathMod.HardMod(mod.Path, Path.Join(LuaEngine.SCRIPT_PATH, mod.Namespace, packagefilepath, STRINGS_FILE_NAME + "." + code + ".resx"));
                    if (File.Exists(modulePath))
                    {
                        Dictionary<string, string> dict = Text.LoadStringResx(modulePath);
                        foreach (string key in dict.Keys)
                        {
                            if (!xmlDict.ContainsKey(key))
                                xmlDict.Add(key, dict[key]);
                        }
                    }
                }

                //then go through all mods of the official fallbacks
                if (Text.LangNames.ContainsKey(code))
                {
                    foreach (string fallback in Text.LangNames[code].Fallbacks)
                    {
                        foreach (ModHeader mod in PathMod.FallbackMods(LuaEngine.SCRIPT_PATH))
                        {
                            string modulePath = PathMod.HardMod(mod.Path, Path.Join(LuaEngine.SCRIPT_PATH, mod.Namespace, packagefilepath, STRINGS_FILE_NAME + "." + fallback + ".resx"));
                            if (File.Exists(modulePath))
                            {
                                Dictionary<string, string> dict = Text.LoadStringResx(modulePath);
                                foreach (string key in dict.Keys)
                                {
                                    if (!xmlDict.ContainsKey(key))
                                        xmlDict.Add(key, dict[key]);
                                }
                            }
                        }
                    }
                }
                //then go through all mods of the default language
                foreach (ModHeader mod in PathMod.FallbackMods(LuaEngine.SCRIPT_PATH))
                {
                    string modulePath = PathMod.HardMod(mod.Path, Path.Join(LuaEngine.SCRIPT_PATH, mod.Namespace, packagefilepath, STRINGS_FILE_NAME + ".resx"));
                    if (File.Exists(modulePath))
                    {
                        Dictionary<string, string> dict = Text.LoadStringResx(modulePath);
                        foreach (string key in dict.Keys)
                        {
                            if (!xmlDict.ContainsKey(key))
                                xmlDict.Add(key, dict[key]);
                        }
                    }
                }


                //Build a lua table as we go and return it
                LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, key, str) tbl[key] = str end").First() as LuaFunction;

                foreach (string name in xmlDict.Keys)
                    addfn.Call(tbl, name, xmlDict[name]);

                return tbl;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            }
        }

        

        /// <summary>
        /// Gets the current language setting of the game.
        /// </summary>
        /// <returns>The current language, represented by a locale code.</returns>
        public string LocaleCode()
        {
            return DiagManager.Instance.CurSettings.Language;
        }

        /// <summary>
        /// Formats a string.  Will unescape escaped characters and process grammar tags.
        /// </summary>
        /// <param name="fmt">String to format.</param>
        /// <param name="para">Arguments</param>
        /// <returns>The formatted string.</returns>
        public string Format( string fmt, params object[] para )
        {
            try
            {
                return Text.FormatGrammar(System.Text.RegularExpressions.Regex.Unescape(fmt), para);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return fmt;
        }

        /// <summary>
        /// Formats a string given a string key.  Will unescape escaped characters.
        /// </summary>
        /// <param name="fmt">The string key to format</param>
        /// <param name="para">string arguments</param>
        /// <returns></returns>
        public string FormatKey(string fmt, params object[] para)
        {
            try
            {
                return Text.FormatKey(fmt, para);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return fmt;
        }

        /// <summary>
        /// Gets the string representing a button or key.
        /// </summary>
        /// <param name="index">The input type of the input.</param>
        /// <returns>The string representing the button o key the input maps to.</returns>
        public string LocalKeyString(int index)
        {
            try
            {
                return DiagManager.Instance.GetControlString((FrameInput.InputType)index);//TODO: LUA 5.3: do regex unescape here
                //return System.Text.RegularExpressions.Regex.Unescape(DiagManager.Instance.CurSettings.ActionKeys[index].ToLocal());
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return ((FrameInput.InputType)index).ToString();
        }

        /// <summary>
        /// Builds a single string of comma separated elements.
        /// </summary>
        /// <param name="listStrings">LuaTable of strings containing the elements to build the list from.</param>
        /// <returns>The combined string containing all elements.</returns>
        public string CreateList(LuaTable listStrings)
        {
            try
            {
                List<string> para = new List<string>();
                foreach (object key in listStrings.Keys)
                {
                    string entry = (string)listStrings[key];
                    para.Add(entry);
                }
                return System.Text.RegularExpressions.Regex.Unescape(Text.BuildList(para.ToArray()));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return "";
        }

        public override void SetupLuaFunctions(LuaEngine state)
        {
            //TODO
        }
    }
}
