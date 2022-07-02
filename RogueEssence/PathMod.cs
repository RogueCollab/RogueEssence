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
        public static string RESOURCE_PATH { get => ASSET_PATH + "Editor/"; }

        public static string MODS_PATH { get => ExePath + MODS_FOLDER; }
        public static string MODS_FOLDER = "MODS/";

        /// <summary>
        /// Filename of mod relative to executable
        /// </summary>
        public static ModHeader[] Mods = new ModHeader[0];

        public static ModHeader Quest = ModHeader.Invalid;

        public static void InitExePath(string path)
        {
            ExeName = Path.GetFileName(path);
            ExePath = Path.GetDirectoryName(path) + "/";
            ASSET_PATH = ExePath;
            DEV_PATH = ExePath + "RawAsset/";
        }

        /// <summary>
        /// Takes a full path and returns a new path that is relative to the executable folder.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRelativePath(string path)
        {
            List<string> exeSplit = Split(ExePath);
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
            return Path.Join(ExePath, baseFolder, Quest.Path);
        }

        public static string ModSavePath(string baseFolder, string basePath)
        {
            return Path.Join(ExePath, baseFolder, Quest.Path, basePath);
        }

        public static string HardMod(string basePath)
        {
            return hardMod(Quest.Path, basePath);
        }
        public static string NoMod(string basePath)
        {
            return Path.Join(ASSET_PATH, basePath);
        }
        public static string FromExe(string basePath)
        {
            return Path.Join(ExePath, basePath);
        }
        private static string hardMod(string mod, string basePath)
        {
            if (mod == "")
                return Path.Join(ASSET_PATH, basePath);
            else
                return Path.Join(ExePath, mod, basePath);
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
            foreach(ModHeader mod in Mods)
            {
                string fullPath = hardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
            }
            if (Quest.IsValid())
            {
                string mod = Quest.Path;
                string fullPath = hardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
            }
            yield return hardMod("", basePath);
        }

        //Temporary fix specifically for lua: check quest path then mod path
        //remove when: loading strings the proper way in script
        public static string QuestPath(string basePath)
        {
            if (Quest.IsValid())
            {
                string mod = Quest.Path;
                string fullPath = hardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
            }

            foreach (ModHeader mod in Mods)
            {
                string fullPath = hardMod(mod.Path, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
            }

            return hardMod("", basePath);
        }

        public static string ModPath(string basePath)
        {
            foreach (string modPath in FallbackPaths(basePath))
                return modPath;

            return hardMod("", basePath);
        }

        public static string[] GetModFiles(string baseFolder, string search = "*")
        {
            List<string[]> files = new List<string[]>();
            foreach (ModHeader mod in Mods)
            {
                string fullPath = hardMod(mod.Path, baseFolder);
                if (Directory.Exists(fullPath))
                    files.Add(Directory.GetFiles(fullPath, search));
            }
            if (Quest.IsValid())
            {
                string mod = Quest.Path;
                string fullPath = hardMod(mod, baseFolder);
                if (Directory.Exists(fullPath))
                    files.Add(Directory.GetFiles(fullPath, search));
            }
            files.Add(Directory.GetFiles(hardMod("", baseFolder), search));

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


        public static ModHeader GetModDetails(string path)
        {
            ModHeader header = ModHeader.Invalid;
            try
            {
                if (!String.IsNullOrEmpty(path))
                {
                    if (Directory.Exists(path))
                    {
                        header.Path = Path.Join(MODS_FOLDER, Path.GetFileName(path));

                        string filePath = Path.Join(path, "Mod.xml");
                        if (File.Exists(filePath))
                        {
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.Load(filePath);
                            header.Name = xmldoc.SelectSingleNode("Header/Name").InnerText;
                            header.UUID = Guid.Parse(xmldoc.SelectSingleNode("Header/UUID").InnerText);
                            header.Version = Version.Parse(xmldoc.SelectSingleNode("Header/Version").InnerText);
                            header.ModType = Enum.Parse<PathMod.ModType>(xmldoc.SelectSingleNode("Header/ModType").InnerText);
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

        public static void SaveModDetails(string path, ModHeader header)
        {
            XmlDocument xmldoc = new XmlDocument();

            XmlNode docNode = xmldoc.CreateElement("Header");
            xmldoc.AppendChild(docNode);

            docNode.AppendInnerTextChild(xmldoc, "Name", header.Name);
            docNode.AppendInnerTextChild(xmldoc, "UUID", header.UUID.ToString().ToUpper());
            docNode.AppendInnerTextChild(xmldoc, "Version", header.Version.ToString());
            docNode.AppendInnerTextChild(xmldoc, "ModType", header.ModType.ToString());

            xmldoc.Save(Path.Join(path, "Mod.xml"));
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

    public struct ModHeader
    {
        /// <summary>
        /// Must always be a relative path
        /// </summary>
        public string Path;
        public string Name;
        public Guid UUID;
        public Version Version;
        public PathMod.ModType ModType;

        public static readonly ModHeader Invalid = new ModHeader("", "", Guid.Empty, new Version(), PathMod.ModType.None);

        public ModHeader(string path, string name, Guid uuid, Version version, PathMod.ModType modType)
        {
            Path = path;
            Name = name;
            UUID = uuid;
            Version = version;
            ModType = modType;
        }

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Path);
        }

        public bool IsFilled()
        {
            return !String.IsNullOrEmpty(Path) && !String.IsNullOrEmpty(Name) && !UUID.Equals(Guid.Empty) && ModType != PathMod.ModType.None;
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
