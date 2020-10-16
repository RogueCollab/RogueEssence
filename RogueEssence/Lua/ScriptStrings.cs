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
        public const string STRINGS_FILE_EXT = "resx";

        public LuaTable MakePackageStringTable(string packagefilepath)
        {
            LuaTable tbl = loadXmlDoc(packagefilepath + "/" + STRINGS_FILE_NAME + "." + LocaleCode() + "." + STRINGS_FILE_EXT);
            LuaTable defaulttbl = loadXmlDoc(packagefilepath + "/" + STRINGS_FILE_NAME + "." + STRINGS_FILE_EXT);
            LuaFunction addmeta = LuaEngine.Instance.RunString("return function(tbl1, tbl2) setmetatable(tbl1, { __index = tbl2 }) end").First() as LuaFunction;
            addmeta.Call(tbl, defaulttbl);
            
            return tbl;
        }

        private LuaTable loadXmlDoc(string path)
        {
            try
            {
                //Build a lua table as we go and return it
                LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, key, str) tbl[key] = str end").First() as LuaFunction;
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
                                addfn.Call(tbl, name, value);
                        }
                    }
                }
                return tbl;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            }
        }

        


        public string LocaleCode()
        {
            return DiagManager.Instance.CurSettings.Language;
        }

        public string Format( string fmt, params object[] para )
        {
            try
            {
                return String.Format(System.Text.RegularExpressions.Regex.Unescape(fmt), para);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            return "";
        }

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
            return "";
        }

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
            return "";
        }
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
