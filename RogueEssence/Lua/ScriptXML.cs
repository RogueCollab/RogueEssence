using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NLua;
using System.IO;

namespace RogueEssence.Script
{
    /*
     * This is meant to provide a memory safe and quick way to load XML files to a lua table for the script engine's use.
    */
    class ScriptXML : ILuaEngineComponent
    {
        LuaFunction InsertChildNodeType;

        public XmlElement LoadXmlFileToTable(string filepath)
        {
            try
            {
                LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;


                if (!File.Exists(filepath))
                    return null;

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(filepath);

                //foreach (XmlNode xnode in xmldoc.DocumentElement.ChildNodes)
                //{
                //    InsertChildNodeType.Call(tbl, xnode.Name, AddNode(xnode));
                //    //LuaEngine.Instance.CallLuaFunctions("table.insert", tbl, AddNode(xnode));
                //}


                return xmldoc.DocumentElement;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
                return null;
            }
        }

        public XmlElement GetXmlNodeNamedChild(XmlNode parent, string nodename)
        {
            return parent[nodename];
        }

        public string GetXmlNodeText(XmlNode node)
        {
            return node.InnerText;
        }


        //private LuaTable AddNode(XmlNode curnode)
        //{
        //    LuaTable curtbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;

        //    if (curnode.HasChildNodes)
        //    {
        //        foreach (XmlNode xnode in curnode.ChildNodes)
        //            curtbl[xnode.Name] = AddNode(xnode);
        //    }
        //    else if(!String.IsNullOrEmpty(curnode.Value))
        //    {
        //        //curtbl = LuaEngine.Instance.RunString(@"
        //        //local tbl = {}
        //        //table.insert(tbl,'" + curnode.Value + @"')
        //        //return tbl
        //        //").First() as LuaTable;

        //        //LuaEngine.Instance.CallLuaFunctions("table.insert", curtbl, curnode.Value);
        //        InsertChildNodeType.Call(curtbl, curnode.Name, AddNode(curnode));
        //    }
        //    return curtbl;
        //}

        public override void SetupLuaFunctions(LuaEngine state)
        {
            InsertChildNodeType = state.RunString("return function(tbl, nodename, value) table.insert( tbl[nodename], value); end").First() as LuaFunction;
        }
    }
}