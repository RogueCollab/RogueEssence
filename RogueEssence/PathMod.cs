using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Diagnostics;
using RogueEssence.Dev;

namespace RogueEssence
{
    public static class PathMod
    {
        public enum ModType
        {
            None=-1,
            Mod,
            Quest,
            Count
        }

        public static string ExeName { get; private set; }
        public static string ExePath { get; private set; }
        public static string ASSET_PATH;
        public static string DEV_PATH;
        public static string APP_PATH;
        public static string RESOURCE_PATH { get => ASSET_PATH + "Editor/"; }
        public static string BASE_PATH { get => ASSET_PATH + "Base/"; }

        public static string MODS_PATH { get => APP_PATH + MODS_FOLDER; }
        public static string MODS_FOLDER = "MODS/";

        public const string PATH_PARAMS_FILE = "PathParams.xml";

        /// <summary>
        /// The namespace for the base game, relative to mods
        /// </summary>
        public static string BaseNamespace { get; private set; }

        /// <summary>
        /// Additional namespaces of the base game, purely for use in scripting
        /// </summary>
        public static List<string> BaseScriptNamespaces { get; private set; }

        /// <summary>
        /// Filename of mod relative to executable
        /// </summary>
        public static ModHeader[] Mods { get; private set; }

        public static ModHeader Quest { get; private set; }

        public static List<int> LoadOrder { get; private set; }

        private static Dictionary<string, ModHeader> namespaceLookup;

        private static Dictionary<Guid, ModHeader> uuidLookup;

        public static void InitPathMod(string path)
        {
            ExeName = Path.GetFileName(path);
            ExePath = Path.GetDirectoryName(path) + "/";

            //Asset path can be changed after initialization
            ASSET_PATH = ExePath;
            //Dev path can be changed after initialization
            DEV_PATH = ExePath + "RawAsset/";
            //App path can be changed after initialization
            APP_PATH = ExePath;

            BaseNamespace = "";
            BaseScriptNamespaces = new List<string>();

            Quest = ModHeader.Invalid;
            Mods = new ModHeader[0];
            LoadOrder = new List<int>();
            namespaceLookup = new Dictionary<string, ModHeader>();
            uuidLookup = new Dictionary<Guid, ModHeader>();
        }

        public static void InitNamespaces()
        {
            string path = Path.Join(PathMod.BASE_PATH, PATH_PARAMS_FILE);

            //try to load from file
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(path);

                XmlNode namespaceNode = xmldoc.DocumentElement.SelectSingleNode("BaseNamespace");
                BaseNamespace = namespaceNode.InnerText;

                {
                    BaseScriptNamespaces = new List<string>();
                    XmlNode scriptNamespaces = xmldoc.DocumentElement.SelectSingleNode("BaseScriptNamespaces");
                    foreach (XmlNode scriptNode in scriptNamespaces.SelectNodes("ScriptNamespace"))
                        BaseScriptNamespaces.Add(scriptNode.InnerText);
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
                throw;
            }
        }

        public static void SaveNamespaces(string destPath, string baseNameSpace, List<string> baseScriptNamespaces)
        {
            string path = Path.Join(destPath, PATH_PARAMS_FILE);
            try
            {
                XmlDocument xmldoc = new XmlDocument();

                XmlNode docNode = xmldoc.CreateElement("root");
                xmldoc.AppendChild(docNode);

                docNode.AppendInnerTextChild(xmldoc, "BaseNamespace", baseNameSpace);

                XmlNode scriptNamespaceNode = xmldoc.CreateElement("BaseScriptNamespaces");
                docNode.AppendChild(scriptNamespaceNode);

                foreach (string scriptNamespace in baseScriptNamespaces)
                    scriptNamespaceNode.AppendInnerTextChild(xmldoc, "ScriptNamespace", scriptNamespace);

                xmldoc.Save(path);
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
        }

        public static void SetMods(ModHeader quest, ModHeader[] mods, List<int> loadOrder)
        {
            Quest = quest;
            Mods = mods;
            LoadOrder = loadOrder;

            namespaceLookup.Clear();
            uuidLookup.Clear();
            for (int ii = LoadOrder.Count - 1; ii >= 0; ii--)
            {
                ModHeader mod = getModHeader(Quest, Mods, LoadOrder[ii]);
                namespaceLookup[mod.Namespace] = mod;
                uuidLookup[mod.UUID] = mod;
            }
        }

        public static ModHeader GetModFromNamespace(string namespaceStr)
        {
            ModHeader header;
            if (namespaceLookup.TryGetValue(namespaceStr, out header))
                return header;

            header = ModHeader.Invalid;
            header.Namespace = BaseNamespace;
            return header;
        }

        public static string GetCurrentNamespace()
        {
            if (Quest.IsValid())
                return Quest.Namespace;

            return BaseNamespace;
        }

        public static ModHeader GetModFromUuid(Guid uuid)
        {
            ModHeader header;
            if (uuidLookup.TryGetValue(uuid, out header))
                return header;

            header = ModHeader.Invalid;
            header.Namespace = BaseNamespace;
            return header;
        }

        /// <summary>
        /// Takes a full path and returns a new path that is relative to the relativeFrom folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRelativePath(string relativeFrom, string path)
        {
            List<string> exeSplit = Split(relativeFrom);
            List<string> split = Split(path);

            List<string> result = new List<string>();
            for (int ii = 0; ii < split.Count; ii++)
            {
                if (ii < exeSplit.Count)
                {
                    if (exeSplit[ii] == split[ii])
                        continue;
                    else
                    {
                        result.Insert(0, "..");
                        result.Add(split[ii]);
                    }
                }
                else
                    result.Add(split[ii]);
            }

            for (int jj = split.Count; jj < exeSplit.Count; jj++)
                result.Insert(0, "..");

            return Path.Combine(result.ToArray());
        }

        public static List<string> Split(string path)
        {
            List<string> result = new List<string>();
            DirectoryInfo pathInfo = new DirectoryInfo(path);
            foreach (string split in Split(pathInfo))
                result.Add(split);
            return result;
        }

        public static IEnumerable<string> Split(this DirectoryInfo path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Parent != null)
                foreach (var d in Split(path.Parent))
                    yield return d;
            yield return path.Name;
        }

        public static string ModSavePath(string baseFolder)
        {
            return Path.Join(APP_PATH, baseFolder, Quest.Path);
        }

        public static string ModSavePath(string baseFolder, string basePath)
        {
            return Path.Join(APP_PATH, baseFolder, Quest.Path, basePath);
        }

        public static string HardMod(string basePath)
        {
            return HardMod(Quest.Path, basePath);
        }
        public static string NoMod(string basePath)
        {
            return Path.Join(ASSET_PATH, basePath);
        }
        public static string FromApp(string basePath)
        {
            return Path.Join(APP_PATH, basePath);
        }
        public static string HardMod(string mod, string basePath)
        {
            if (mod == "")
                return Path.Join(ASSET_PATH, basePath);
            else
                return Path.Join(APP_PATH, mod, basePath);
        }


        /// <summary>
        /// Scripts are the ONLY place that use the additional script namespaces, so a special iterator must be used for them.
        /// </summary>
        public static IEnumerable<ModHeader> FallforthScriptMods(string basePath)
        {
            Stack<ModHeader> mods = new Stack<ModHeader>();
            foreach (ModHeader mod in FallbackScriptMods(basePath))
                mods.Push(mod);

            while (mods.Count > 0)
                yield return mods.Pop();
        }

        /// <summary>
        /// Scripts are the ONLY place that use the additional script namespaces, so a special iterator must be used for them.
        /// </summary>
        public static IEnumerable<ModHeader> FallbackScriptMods(string basePath)
        {
            for (int ii = LoadOrder.Count - 1; ii >= 0; ii--)
            {
                ModHeader mod = getModHeader(Quest, Mods, LoadOrder[ii]);
                string fullPath = HardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return mod;
            }


            for (int ii = BaseScriptNamespaces.Count - 1; ii >= 0; ii--)
            {
                ModHeader header = ModHeader.Invalid;
                header.Namespace = BaseScriptNamespaces[ii];
                yield return header;
            }

            {
                ModHeader header = ModHeader.Invalid;
                header.Namespace = BaseNamespace;
                yield return header;
            }
        }

        public static IEnumerable<ModHeader> FallbackMods(string basePath)
        {
            for (int ii = LoadOrder.Count - 1; ii >= 0; ii--)
            {
                ModHeader mod = getModHeader(Quest, Mods, LoadOrder[ii]);
                string fullPath = HardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return mod;
            }

            ModHeader header = ModHeader.Invalid;
            header.Namespace = BaseNamespace;
            yield return header;
        }

        public static IEnumerable<string> FallforthPaths(string basePath)
        {
            Stack<string> paths = new Stack<string>();
            foreach (string path in FallbackPaths(basePath))
                paths.Push(path);

            while (paths.Count > 0)
                yield return paths.Pop();
        }

        public static IEnumerable<string> FallbackPaths(string basePath)
        {
            for (int ii = LoadOrder.Count - 1; ii >= 0; ii--)
            {
                ModHeader mod = getModHeader(Quest, Mods, LoadOrder[ii]);
                string fullPath = HardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
            }

            yield return HardMod("", basePath);
        }

        public static IEnumerable<(ModHeader, string)> FallforthPathsWithHeader(string basePath)
        {
            string baseFullPath = HardMod("", basePath);
            if (File.Exists(baseFullPath) || Directory.Exists(baseFullPath))
                yield return (ModHeader.Invalid, baseFullPath);

            for (int ii = 0; ii < LoadOrder.Count; ii++)
            {
                ModHeader mod = getModHeader(Quest, Mods, LoadOrder[ii]);
                string fullPath = HardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return (mod, fullPath);
            }
        }

        //Temporary fix specifically for lua: check quest path then mod path
        //remove when: loading strings the proper way in script
        public static string QuestPath(string basePath)
        {
            if (Quest.IsValid())
            {
                string mod = Quest.Path;
                string fullPath = HardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
            }

            foreach (ModHeader mod in Mods)
            {
                string fullPath = HardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
            }

            return HardMod("", basePath);
        }

        public static string ModPath(string basePath)
        {
            foreach (string modPath in FallbackPaths(basePath))
                return modPath;

            return HardMod("", basePath);
        }

        public static string[] GetHardModFiles(string baseFolder, string search = "*")
        {
            if (Quest.IsValid())
            {
                string mod = Quest.Path;
                string fullPath = HardMod(mod, baseFolder);
                if (Directory.Exists(fullPath))
                    return Directory.GetFiles(fullPath, search);
                return new string[0];
            }
            else
                return Directory.GetFiles(HardMod("", baseFolder), search);
        }

        public static string[] GetModFiles(string baseFolder, string search = "*")
        {
            List<string[]> files = new List<string[]>();
            for (int ii = LoadOrder.Count - 1; ii >= 0; ii--)
            {
                ModHeader mod = getModHeader(Quest, Mods, LoadOrder[ii]);
                string fullPath = HardMod(mod.Path, baseFolder);
                if (Directory.Exists(fullPath))
                    files.Add(Directory.GetFiles(fullPath, search));
            }

            files.Add(Directory.GetFiles(HardMod("", baseFolder), search));

            Dictionary<string, string> file_mappings = new Dictionary<string, string>();
            for (int ii = files.Count-1; ii >= 0; ii--)
            {
                for (int jj = 0; jj < files[ii].Length; jj++)
                {
                    string full_path = files[ii][jj];
                    string file_path = Path.GetFileName(full_path);
                    file_mappings[file_path] = full_path;
                }
            }

            List<string> result_files = new List<string>();
            foreach (string str in file_mappings.Keys)
                result_files.Add(str);
            result_files.Sort();
            for (int ii = 0; ii < result_files.Count; ii++)
                result_files[ii] = file_mappings[result_files[ii]];
            return result_files.ToArray();
        }

        private static ModHeader getModHeader(ModHeader quest, ModHeader[] mods, int idx)
        {
            if (idx == -1)
                return quest;
            return mods[idx];
        }

        public static void ValidateModLoad(ModHeader quest, ModHeader[] mods, List<int> loadOrder, List<(ModRelationship, List<ModHeader>)> loadErrors)
        {
            Dictionary<Guid, int> guidLookup = new Dictionary<Guid, int>();
            Dictionary<int, HashSet<int>> localOrders = new Dictionary<int, HashSet<int>>();

            List<int> preOrder = new List<int>();
            if (quest.IsValid())
            {
                preOrder.Add(-1);
                guidLookup[quest.UUID] = -1;
                localOrders[-1] = new HashSet<int>();
            }
            for (int ii = 0; ii < mods.Length; ii++)
            {
                preOrder.Add(ii);
                guidLookup[mods[ii].UUID] = ii;
                localOrders[ii] = new HashSet<int>();
            }

            //construct the dependency table
            //and also take out the simple missing dependency/incompatibilities
            for (int ii = 0; ii < preOrder.Count; ii++)
            {
                ModHeader header = getModHeader(quest, mods, preOrder[ii]);
                if (Versioning.GetVersion() < header.GameVersion)
                    loadErrors.Add((ModRelationship.DependsOn, new List<ModHeader>() { header, new ModHeader("", "", "", "", "", Guid.Empty, header.GameVersion, new Version(), ModType.None, new RelatedMod[0] { }) }));

                foreach (RelatedMod rel in header.Relationships)
                {
                    switch (rel.Relationship)
                    {
                        case ModRelationship.Incompatible:
                            {
                                int depIdx;
                                if (guidLookup.TryGetValue(rel.UUID, out depIdx))
                                    loadErrors.Add((ModRelationship.Incompatible, new List<ModHeader>() { header, getModHeader(quest, mods, depIdx) }));
                            }
                            break;
                        case ModRelationship.LoadBefore:
                            {
                                int depIdx;
                                if (guidLookup.TryGetValue(rel.UUID, out depIdx))
                                    localOrders[depIdx].Add(preOrder[ii]);
                            }
                            break;
                        case ModRelationship.LoadAfter:
                            {
                                int depIdx;
                                if (guidLookup.TryGetValue(rel.UUID, out depIdx))
                                    localOrders[preOrder[ii]].Add(depIdx);
                            }
                            break;
                        case ModRelationship.DependsOn:
                            {
                                int depIdx;
                                if (guidLookup.TryGetValue(rel.UUID, out depIdx))
                                    localOrders[preOrder[ii]].Add(depIdx);
                                else
                                    loadErrors.Add((ModRelationship.DependsOn, new List<ModHeader>() { header, new ModHeader("", "", "", "", rel.Namespace, rel.UUID, new Version(), new Version(), ModType.None, new RelatedMod[0] { }) }));
                            }
                            break;
                    }
                }
            }

            //iterate the list of items for the ones without dependencies
            //place them in the result list.  mark them as managed
            //iterate the list of items for the ones that refer to only marked items
            //place them in the result list
            //keep doing this until all items are marked, or all unmarked items have at least one unmarked dep
            HashSet<int> marked = new HashSet<int>();

            bool added = true;
            while (added)
            {
                added = false;
                for (int ii = 0; ii < preOrder.Count; ii++)
                {
                    if (!marked.Contains(preOrder[ii]))
                    {
                        bool allMarked = true;
                        foreach (int dep in localOrders[preOrder[ii]])
                        {
                            if (!marked.Contains(dep))
                                allMarked = false;
                        }
                        if (allMarked)
                        {
                            loadOrder.Add(preOrder[ii]);
                            marked.Add(preOrder[ii]);
                            added = true;
                        }
                    }
                }
            }

            //we're done with all the dependencies we can.  the rest are parts of a cycle
            List<ModHeader> cycleHeaders = new List<ModHeader>();
            for (int ii = 0; ii < preOrder.Count; ii++)
            {
                if (!marked.Contains(preOrder[ii]))
                {
                    cycleHeaders.Add(getModHeader(quest, mods, preOrder[ii]));
                    loadOrder.Add(preOrder[ii]);
                }
            }
            if (cycleHeaders.Count > 0)
                loadErrors.Add((ModRelationship.LoadAfter, cycleHeaders));
        }



        public static List<ModHeader> GetEligibleMods(ModType modType)
        {
            List<ModHeader> mods = new List<ModHeader>();
            string[] files = Directory.GetDirectories(MODS_PATH);

            foreach (string modPath in files)
            {
                string mod = Path.GetFileNameWithoutExtension(modPath);
                if (mod != "")
                {
                    //check the config for mod type of Mod
                    ModHeader header = GetModDetails(modPath);

                    if (header.IsValid() && header.ModType == modType)
                        mods.Add(header);
                }
            }
            return mods;
        }

        public static ModHeader GetModDetails(string fullPath)
        {
            ModHeader header = ModHeader.Invalid;
            try
            {
                if (!String.IsNullOrEmpty(fullPath))
                {
                    if (Directory.Exists(fullPath))
                    {
                        header.Path = Path.Join(MODS_FOLDER, Path.GetFileName(fullPath));

                        string filePath = Path.Join(fullPath, "Mod.xml");
                        if (File.Exists(filePath))
                        {
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.Load(filePath);
                            header.Name = xmldoc.SelectSingleNode("Header/Name").InnerText;

                            //TODO: v1.1 remove null check
                            XmlNode authorNode = xmldoc.SelectSingleNode("Header/Author");
                            if (authorNode != null)
                                header.Author = xmldoc.SelectSingleNode("Header/Author").InnerText;

                            //TODO: v1.1 remove null check
                            XmlNode descNode = xmldoc.SelectSingleNode("Header/Description");
                            if (descNode != null)
                                header.Description = xmldoc.SelectSingleNode("Header/Description").InnerText;

                            //TODO: v1.1 remove this
                            XmlNode namespaceNode = xmldoc.SelectSingleNode("Header/Namespace");
                            if (namespaceNode != null)
                                header.Namespace = xmldoc.SelectSingleNode("Header/Namespace").InnerText;
                            else
                                header.Namespace = Text.Sanitize(header.Name).ToLower();
                            header.UUID = Guid.Parse(xmldoc.SelectSingleNode("Header/UUID").InnerText);
                            header.Version = Version.Parse(xmldoc.SelectSingleNode("Header/Version").InnerText);

                            //TODO: v1.1 remove this
                            XmlNode gameVersionNode = xmldoc.SelectSingleNode("Header/GameVersion");
                            if (gameVersionNode != null)
                                header.GameVersion = Version.Parse(gameVersionNode.InnerText);

                            header.ModType = Enum.Parse<PathMod.ModType>(xmldoc.SelectSingleNode("Header/ModType").InnerText);

                            //TODO: v1.1 remove this
                            XmlNode relationNode = xmldoc.SelectSingleNode("Header/Relationships");
                            if (relationNode != null)
                            {
                                List<RelatedMod> relations = new List<RelatedMod>();
                                foreach (XmlNode relatedNode in relationNode.SelectNodes("RelatedMod"))
                                {
                                    RelatedMod relation = new RelatedMod();
                                    relation.UUID = Guid.Parse(relatedNode.SelectSingleNode("UUID").InnerText);
                                    relation.Namespace = relatedNode.SelectSingleNode("Namespace").InnerText;
                                    relation.Relationship = Enum.Parse<ModRelationship>(relatedNode.SelectSingleNode("Relationship").InnerText);
                                    relations.Add(relation);
                                }
                                header.Relationships = relations.ToArray();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, false);
            }

            return header;
        }

        public static void SaveModDetails(string fullPath, ModHeader header)
        {
            XmlDocument xmldoc = new XmlDocument();

            XmlNode docNode = xmldoc.CreateElement("Header");
            xmldoc.AppendChild(docNode);

            docNode.AppendInnerTextChild(xmldoc, "Name", header.Name);
            docNode.AppendInnerTextChild(xmldoc, "Author", header.Author);
            docNode.AppendInnerTextChild(xmldoc, "Description", header.Description);
            docNode.AppendInnerTextChild(xmldoc, "Namespace", header.Namespace);
            docNode.AppendInnerTextChild(xmldoc, "UUID", header.UUID.ToString().ToUpper());
            docNode.AppendInnerTextChild(xmldoc, "Version", header.Version.ToString());
            docNode.AppendInnerTextChild(xmldoc, "GameVersion", header.GameVersion.ToString());
            docNode.AppendInnerTextChild(xmldoc, "ModType", header.ModType.ToString());


            XmlNode relationships = xmldoc.CreateElement("Relationships");
            foreach (RelatedMod relation in header.Relationships)
            {
                XmlNode node = xmldoc.CreateElement("RelatedMod");
                node.AppendInnerTextChild(xmldoc, "UUID", relation.UUID.ToString().ToUpper());
                node.AppendInnerTextChild(xmldoc, "Namespace", relation.Namespace);
                node.AppendInnerTextChild(xmldoc, "Relationship", relation.Relationship.ToString());
                relationships.AppendChild(node);
            }
            docNode.AppendChild(relationships);

            xmldoc.Save(Path.Join(fullPath, "Mod.xml"));

            //TODO: generate a README.md
        }



        public static List<ModVersion> GetModVersion()
        {
            List<ModVersion> result = new List<ModVersion>();
            result.Add(new ModVersion("[Game]", Guid.Empty, Versioning.GetVersion()));
            if (Quest.IsValid())
                result.Add(new ModVersion(Quest.Name, Quest.UUID, Quest.Version));

            foreach (ModHeader mod in Mods)
                result.Add(new ModVersion(mod.Name, mod.UUID, mod.Version));

            return result;
        }

        /// <summary>
        /// Returns a diff list of mod versions preserving order as much as possible.
        /// </summary>
        /// <param name="oldVersion"></param>
        /// <param name="newVersion"></param>
        /// <returns></returns>
        public static List<ModDiff> DiffModVersions(List<ModVersion> oldVersion, List<ModVersion> newVersion)
        {
            HashSet<Guid> processedIDs = new HashSet<Guid>();
            List<ModDiff> result = new List<ModDiff>();

            for (int ii = 0; ii < oldVersion.Count; ii++)
            {
                if (!processedIDs.Contains(oldVersion[ii].UUID))
                {
                    bool found = false;
                    for (int jj = ii; jj < newVersion.Count; jj++)
                    {
                        if (oldVersion[ii].UUID == newVersion[jj].UUID)
                        {
                            if (oldVersion[ii].Version != newVersion[jj].Version)
                                result.Add(new ModDiff(newVersion[jj].Name, newVersion[jj].UUID, oldVersion[ii].Version, newVersion[jj].Version));
                            found = true;
                        }
                    }
                    if (!found)
                        result.Add(new ModDiff(oldVersion[ii].Name, oldVersion[ii].UUID, oldVersion[ii].Version, null));
                    processedIDs.Add(oldVersion[ii].UUID);
                }

                if (ii < newVersion.Count && !processedIDs.Contains(newVersion[ii].UUID))
                {
                    bool found = false;
                    for (int jj = ii; jj < oldVersion.Count; jj++)
                    {
                        if (oldVersion[jj].UUID == newVersion[ii].UUID)
                        {
                            if (oldVersion[jj].Version != newVersion[ii].Version)
                                result.Add(new ModDiff(newVersion[ii].Name, newVersion[ii].UUID, oldVersion[jj].Version, newVersion[ii].Version));
                            found = true;
                        }
                    }
                    if (!found)
                        result.Add(new ModDiff(newVersion[ii].Name, newVersion[ii].UUID, null, newVersion[ii].Version));
                    processedIDs.Add(newVersion[ii].UUID);
                }
            }

            for (int jj = oldVersion.Count; jj < newVersion.Count; jj++)
            {
                if (!processedIDs.Contains(newVersion[jj].UUID))
                    result.Add(new ModDiff(newVersion[jj].Name, newVersion[jj].UUID, null, newVersion[jj].Version));
            }

            return result;
        }
    }

    public struct ModVersion
    {
        public string Name;
        public Guid UUID;
        public Version Version;
        public string VersionString { get { return (Version == null) ? "---" : Version.ToString(); } }

        public ModVersion(string name, Guid uuid, Version version)
        {
            Name = name;
            UUID = uuid;
            Version = version;
        }
    }

    public struct ModDiff
    {
        public string Name;
        public Guid UUID;
        public Version OldVersion;
        public Version NewVersion;

        public string OldVersionString { get { return (OldVersion == null) ? "---" : OldVersion.ToString(); } }

        public string NewVersionString { get { return (NewVersion == null) ? "---" : NewVersion.ToString(); } }

        public ModDiff(string name, Guid uuid, Version oldVersion, Version newVersion)
        {
            Name = name;
            UUID = uuid;
            OldVersion = oldVersion;
            NewVersion = newVersion;
        }
    }

    public enum ModRelationship
    {
        Incompatible,
        LoadAfter,
        LoadBefore,
        DependsOn
    }

    public struct RelatedMod
    {
        public Guid UUID;
        public string Namespace;
        public ModRelationship Relationship;

        public override string ToString()
        {
            return String.Format("{0}: {1}", Relationship.ToString(), Namespace);
        }
    }

    public struct ModHeader
    {
        /// <summary>
        /// Must always be a relative path
        /// </summary>
        public string Path;
        public string Name;
        public string Author;
        public string Description;
        public string Namespace;
        public Guid UUID;
        public Version Version;
        public Version GameVersion;
        public PathMod.ModType ModType;
        public RelatedMod[] Relationships;

        public static readonly ModHeader Invalid = new ModHeader("", "", "", "", "", Guid.Empty, new Version(), new Version(), PathMod.ModType.None, new RelatedMod[0] { });

        public ModHeader(string path, string name, string author, string description, string newNamespace, Guid uuid, Version version, Version gameVersion, PathMod.ModType modType, RelatedMod[] relationships)
        {
            Path = path;
            Name = name;
            Author = author;
            Description = description;
            Namespace = newNamespace;
            UUID = uuid;
            Version = version;
            GameVersion = gameVersion;
            ModType = modType;
            Relationships = relationships;
        }

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Path);
        }

        public bool IsFilled()
        {
            return !String.IsNullOrEmpty(Path) && !String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(Namespace) && !UUID.Equals(Guid.Empty) && ModType != PathMod.ModType.None;
        }

        public string GetMenuName()
        {
            if (!IsValid())
                return "";

            if (!String.IsNullOrEmpty(Name))
                return Name;

            return System.IO.Path.GetFileName(Path);
        }
    }
}
