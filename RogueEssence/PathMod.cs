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
        public static string ExePath { get; private set; }
        public static string ASSET_PATH;
        public static string DEV_PATH;
        public static string RESOURCE_PATH { get => ASSET_PATH + "Editor/"; }

        public static string MODS_PATH { get => ExePath + MODS_FOLDER; }

        public static string MODS_FOLDER = "MODS/";

        /// <summary>
        /// Filename of mod relative to executable
        /// </summary>
        public static string Mod = "";

        public static void InitExePath(string path)
        {
            ExePath = path + "/";
            ASSET_PATH = ExePath;
            DEV_PATH = ExePath + "RawAsset/";
        }

        public static string ModSavePath(string baseFolder)
        {
            return Path.Join(ExePath, baseFolder, Mod);
        }
        public static string ModSavePath(string baseFolder, string basePath)
        {
            return Path.Join(ExePath, baseFolder, Mod, basePath);
        }
        public static string HardMod(string basePath)
        {
            return hardMod(Mod, basePath);
        }
        public static string NoMod(string basePath)
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

        public static IEnumerable<string> FallbackPaths(string basePath)
        {
            string mod = Mod;
            while (mod != "")
            {
                string fullPath = hardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
                mod = GetModParentPath(mod);
            }
            yield return hardMod(mod, basePath);
        }

        public static string ModPath(string basePath)
        {
            string mod = Mod;
            while (mod != "")
            {
                string fullPath = hardMod(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
                mod = GetModParentPath(mod);
            }
            return hardMod(mod, basePath);
        }
        public static string[] GetModFiles(string baseFolder, string search = "*")
        {
            List<string[]> files = new List<string[]>();
            string mod = Mod;
            while (mod != "")
            {
                string fullPath = hardMod(mod, baseFolder);
                if (Directory.Exists(fullPath))
                    files.Add(Directory.GetFiles(fullPath, search));
                mod = GetModParentPath(mod);
            }
            files.Add(Directory.GetFiles(hardMod(mod, baseFolder), search));

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

        public static string GetModParentPath(string mod)
        {
            string relativeTo = Path.GetFullPath(".");
            string parent = Path.Join(Path.GetFullPath(mod), "..", "..");
            string result = Path.GetRelativePath(relativeTo, parent);
            if (result == ".")
                result = "";
            return result;
        }
    }
}
