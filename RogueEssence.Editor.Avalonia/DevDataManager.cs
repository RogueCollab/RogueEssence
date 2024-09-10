using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Avalonia;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class DevDataManager
    {

        private static LRUCache<TileAddr, Bitmap> tileCache;
        private static LRUCache<string, Bitmap> tilesetCache;
        
        private static Dictionary<string, Bitmap> elementIcons;
        private static Dictionary<BattleData.SkillCategory, Bitmap> skillCategoryIcons;

        private static LRUCache<string, Dictionary<int, string>> aliasCache;

        public static Bitmap IconO;
        public static Bitmap IconX;

        public static List<CharSheetOp> CharSheetOps;

        private static Dictionary<string, string> memberDocs;
        private static Dictionary<string, string> typeDocs;

        private static Dictionary<string, Size> savedTypeSizes;

        public static void Init()
        {
            initEditorSettings();
            initDocs();

            CharSheetOps = new List<CharSheetOp>();

            if (!Directory.Exists(Path.Combine(PathMod.RESOURCE_PATH, "Extensions")))
                Directory.CreateDirectory(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"));
            if (!Directory.Exists(Path.Combine(PathMod.RESOURCE_PATH, "UI")))
                Directory.CreateDirectory(Path.Combine(PathMod.RESOURCE_PATH, "UI"));

            foreach (string path in Directory.GetFiles(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"), "*.op"))
            {
                try
                {
                    CharSheetOp newOp = Data.DataManager.LoadObject<CharSheetOp>(path);
                    CharSheetOps.Add(newOp);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }


            IconO = new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "O.png"));
            IconX = new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "X.png"));
            
            elementIcons = new Dictionary<string, Bitmap>();
            foreach (string element in DataManager.Instance.DataIndices[DataManager.DataType.Element].GetOrderedKeys(true))
            {
                string path = Path.Combine(PathMod.RESOURCE_PATH, "UI", "Element", element + ".png");
                if (File.Exists(path))
                {
                    Bitmap img = new Bitmap(path);
                    elementIcons.Add(element, img);
                }
            }
            
            skillCategoryIcons = new Dictionary<BattleData.SkillCategory, Bitmap>();
            skillCategoryIcons.Add(BattleData.SkillCategory.None, new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "SkillCategory", "None.png")));
            skillCategoryIcons.Add(BattleData.SkillCategory.Physical, new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "SkillCategory", "Physical.png")));
            skillCategoryIcons.Add(BattleData.SkillCategory.Magical, new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "SkillCategory", "Magical.png")));
            skillCategoryIcons.Add(BattleData.SkillCategory.Status, new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "SkillCategory", "Status.png")));
            
            tileCache = new LRUCache<TileAddr, Bitmap>(2000);
            tilesetCache = new LRUCache<string, Bitmap>(10);
            aliasCache = new LRUCache<string, Dictionary<int, string>>(20);
        }

        private static void initEditorSettings()
        {
            savedTypeSizes = new Dictionary<string, Size>();

            string path = Path.Combine(DiagManager.CONFIG_PATH, "EditWindow.xml");

            if (!File.Exists(path))
                return;

            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(path);
                XmlNode members = xmldoc.SelectSingleNode("members");
                foreach (XmlNode xnode in members.ChildNodes)
                {
                    if (xnode.Name == "member")
                    {
                        Size? value = null;
                        string name = null;
                        var atname = xnode.Attributes["name"];
                        if (atname != null)
                            name = atname.Value;

                        //Get value
                        XmlNode valnode = xnode.SelectSingleNode("size");
                        if (valnode != null)
                        {
                            string[] valueStr = valnode.InnerText.Trim().Split(",");
                            if (valueStr.Length == 2)
                            {
                                int width, height;
                                if (int.TryParse(valueStr[0], out width) && int.TryParse(valueStr[1], out height))
                                    value = new Size(width, height);
                            }
                        }

                        if (value != null && name != null)
                            savedTypeSizes[name] = value.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public static void SaveEditorSettings()
        {
            try
            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("members");
                xmldoc.AppendChild(docNode);

                foreach (string typeString in savedTypeSizes.Keys)
                {
                    Size size = savedTypeSizes[typeString];
                    XmlElement member = xmldoc.CreateElement("member");
                    member.SetAttribute("name", typeString);
                    member.AppendInnerTextChild(xmldoc, "size", String.Format("{0},{1}", (int)size.Width, (int)size.Height));
                    docNode.AppendChild(member);
                }

                xmldoc.Save(DiagManager.CONFIG_PATH + "EditWindow.xml");

            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        private static void initDocs()
        {
            memberDocs = new Dictionary<string, string>();
            typeDocs = new Dictionary<string, string>();

            foreach (string path in Directory.GetFiles(Path.Combine(PathMod.RESOURCE_PATH, "Docs"), "*.xml"))
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(path);
                    XmlNode assembly = xmldoc.SelectSingleNode("doc/assembly/name");
                    XmlNode members = xmldoc.SelectSingleNode("doc/members");
                    foreach (XmlNode xnode in members.ChildNodes)
                    {
                        if (xnode.Name == "member")
                        {
                            string value = null;
                            string name = null;
                            var atname = xnode.Attributes["name"];
                            if (atname != null)
                                name = atname.Value;

                            //Get value
                            XmlNode valnode = xnode.SelectSingleNode("summary");
                            if (valnode != null)
                                value = valnode.InnerText.Trim().Replace("\r\n            ", "\r\n");

                            if (value != null && name != null)
                            {
                                if (name.StartsWith("F:") || name.StartsWith("P:"))
                                    memberDocs[assembly.InnerText + ":" + name.Substring(2)] = value;
                                else if (name.StartsWith("T:"))
                                    typeDocs[assembly.InnerText + ":" + name.Substring(2)] = value;

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }
        }

        public static string GetTypeDoc(Type ownerType)
        {
            string desc;
            Type unconstructedType = ownerType;
            if (unconstructedType.IsConstructedGenericType)
                unconstructedType = unconstructedType.GetGenericTypeDefinition();
            string key = unconstructedType.Assembly.GetName().Name + ":" + unconstructedType.FullName;
            if (typeDocs.TryGetValue(key, out desc))
                return String.Format("Full Name: {0}\n{1}", unconstructedType.FullName, desc);

            return String.Format("Full Name: {0}", unconstructedType.FullName);
        }

        public static string GetMemberDoc(Type ownerType, string name)
        {
            Type objectType = typeof(object);
            Type baseType = ownerType;

            string desc;
            //Main base types
            while (baseType != objectType)
            {
                Type unconstructedType = baseType;
                if (unconstructedType.IsConstructedGenericType)
                    unconstructedType = unconstructedType.GetGenericTypeDefinition();
                string key = unconstructedType.Assembly.GetName().Name + ":" + unconstructedType.FullName.Replace('+', '.') + "." + name;
                if (memberDocs.TryGetValue(key, out desc))
                    return desc;
                baseType = baseType.BaseType;
            }

            //Interfaces
            Type[] interfaceTypes = ownerType.GetInterfaces();
            foreach (Type iType in interfaceTypes)
            {
                Type unconstructedType = iType;
                if (unconstructedType.IsConstructedGenericType)
                    unconstructedType = unconstructedType.GetGenericTypeDefinition();
                string key = unconstructedType.Assembly.GetName().Name + ":" + unconstructedType.FullName.Replace('+', '.') + "." + name;
                if (memberDocs.TryGetValue(key, out desc))
                    return desc;
            }

            return null;
        }

        public static bool GetTypeSize(Type type, out Size size)
        {
            string name = type.Assembly.GetName().Name + ":" + type.FullName;
            return savedTypeSizes.TryGetValue(name, out size);
        }

        public static void SetTypeSize(Type type, Size size)
        {
            string name = type.Assembly.GetName().Name + ":" + type.FullName;
            savedTypeSizes[name] = size;
        }

        public static Bitmap GetTypeIcon(string type)
        {
            Bitmap icon;
            if (elementIcons.TryGetValue(type, out icon))
            {
                return icon;

            }
            elementIcons.TryGetValue("none", out icon);
            return icon;
        }
        
        public static Bitmap GetCategoryIcon(BattleData.SkillCategory category)
        {
            Bitmap icon;
            if (skillCategoryIcons.TryGetValue(category, out icon))
            {
                return icon;

            }
            return icon;
        }
        

        public static Bitmap GetTile(TileFrame tileTex)
        {

            long tilePos = GraphicsManager.TileIndex.GetPosition(tileTex.Sheet, tileTex.TexLoc);
            TileAddr addr = new TileAddr(tilePos, tileTex.Sheet);

            Bitmap sheet;
            if (tileCache.TryGetValue(addr, out sheet))
                return sheet;

            if (tilePos > 0)
            {
                try
                {
                    using (FileStream stream = new FileStream(PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, tileTex.Sheet)), FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Seek to the location of the tile
                        stream.Seek(tilePos, SeekOrigin.Begin);

                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            //usually handled by basesheet, cheat a little here
                            long length = reader.ReadInt64();
                            byte[] tileBytes = reader.ReadBytes((int)length);

                            using (MemoryStream tileStream = new MemoryStream(tileBytes))
                                sheet = new Bitmap(tileStream);
                        }
                    }

                    tileCache.Add(addr, sheet);
                    return sheet;
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(new Exception("Error retrieving tile " + tileTex.TexLoc.X + ", " + tileTex.TexLoc.Y + " from Tileset #" + tileTex.Sheet + "\n", ex));
                }
            }

            tileCache.Add(addr, null);
            return null;
        }

        public static void ClearCaches()
        {
            tileCache.Clear();
            tilesetCache.Clear();
        }

        public static Bitmap GetTileset(string tileset)
        {
            Bitmap sheet;
            if (tilesetCache.TryGetValue(tileset, out sheet))
                return sheet;

            try
            {
                DevForm.ExecuteOrPend(() => { loadCacheTileset(tileset); });

                if (tilesetCache.TryGetValue(tileset, out sheet))
                    return sheet;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error retrieving Tileset #" + tileset + "\n", ex));
            }

            tilesetCache.Add(tileset, null);
            return null;
        }

        private static void loadCacheTileset(string tileset)
        {
            Bitmap sheet;
            using (MemoryStream tileStream = new MemoryStream())
            {
                ImportHelper.WriteFullTilesetTexture(PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, tileset)), tileStream);
                tileStream.Seek(0, SeekOrigin.Begin);
                sheet = new Bitmap(tileStream);
            }

            tilesetCache.Add(tileset, sheet);
        }

        public static Dictionary<int, string> GetAlias(string name)
        {
            Dictionary<int, string> alias;
            if (aliasCache.TryGetValue(name, out alias))
                return alias;

            try
            {
                string path = Path.Combine(PathMod.RESOURCE_PATH, "Aliases", name + ".json");
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    alias = (Dictionary<int, string>)Data.Serializer.Deserialize(stream, typeof(Dictionary<int, string>));

                aliasCache.Add(name, alias);
                return alias;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error retrieving Alias \"" + name + "\"\n", ex));
            }
            return null;
        }
    }
}
