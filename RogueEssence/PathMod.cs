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
            Quest,
            Mod,
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
        public static string[] Mods = new string[0];

        public static string Quest = "";

        public static void InitExePath(string path)
        {
            ExeName = Path.GetFileName(path);
            ExePath = Path.GetDirectoryName(path) + "/";
            ASSET_PATH = ExePath;
            DEV_PATH = ExePath + "RawAsset/";
        }

        public static string ModSavePath(string baseFolder)
        {
            return Path.Join(ExePath, baseFolder, Quest);
        }

        public static string ModSavePath(string baseFolder, string basePath)
        {
            return Path.Join(ExePath, baseFolder, Quest, basePath);
        }

        public static string HardMod(string basePath)
        {
            return hardMod(Quest, basePath);
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
            foreach(string mod in Mods)
            {
                string fullPath = hardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
            }
            if (Quest != "")
            {
                string mod = Quest;
                string fullPath = hardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
            }
            yield return hardMod("", basePath);
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
            foreach (string mod in Mods)
            {
                string fullPath = hardMod(mod, baseFolder);
                if (Directory.Exists(fullPath))
                    files.Add(Directory.GetFiles(fullPath, search));
            }
            if (Quest != "")
            {
                string mod = Quest;
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
    }

    public struct ModHeader
    {
        public string Name;
        public Guid UUID;
        public Version Version;
        public PathMod.ModType ModType;

        public static readonly ModHeader Invalid = new ModHeader("", Guid.Empty, new Version(), PathMod.ModType.None);

        public ModHeader(string name, Guid uuid, Version version, PathMod.ModType modType)
        {
            Name = name;
            UUID = uuid;
            Version = version;
            ModType = modType;
        }

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Name) && !UUID.Equals(Guid.Empty) && ModType != PathMod.ModType.None;
        }
    }
}
