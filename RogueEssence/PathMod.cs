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
#if !DEBUG && !PROFILING
        public const string ASSET_PATH = "";
        public const string DEV_PATH = "DevContent/";
        public const string TEMP_PATH = "temp/";
#else
        public const string ASSET_PATH = "../../../../Asset/";
        public const string DEV_PATH = "../../../../RawAsset/";
        public const string TEMP_PATH = "../../../../temp/";
#endif
        public const string MODS_PATH = "MODS/";

        public static string Mod = "";

        //TODO: get the absolute path from the executable for all path usage in this library
        //https://stackoverflow.com/questions/1658518/getting-the-absolute-path-of-the-executable-using-c
        //https://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path

        public static string ModSavePath(string baseFolder)
        {
            return Path.Join(baseFolder, Mod);
        }
        public static string ModSavePath(string baseFolder, string basePath)
        {
            return Path.Join(baseFolder, Mod, basePath);
        }
        public static string HardMod(string basePath)
        {
            if (Mod == "")
                return Path.Join(ASSET_PATH, basePath);
            else
                return Path.Join(Mod, basePath);
        }

        public static IEnumerable<string> FallbackPaths(string basePath)
        {
            string mod = Mod;
            while (mod != "")
            {
                string fullPath = Path.Join(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    yield return fullPath;
                mod = GetModParentPath(mod);
            }
            yield return Path.Join(ASSET_PATH, basePath);
        }

        public static string ModPath(string basePath)
        {
            string mod = Mod;
            while (mod != "")
            {
                string fullPath = Path.Join(mod, basePath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
                mod = GetModParentPath(mod);
            }
            return Path.Join(ASSET_PATH, basePath);
        }
        public static string[] GetModFiles(string baseFolder, string search = "*")
        {
            List<string[]> files = new List<string[]>();
            string mod = Mod;
            while (mod != "")
            {
                string fullPath = Path.Join(mod, baseFolder);
                if (Directory.Exists(fullPath))
                    files.Add(Directory.GetFiles(fullPath, search));
                mod = GetModParentPath(mod);
            }
            files.Add(Directory.GetFiles(Path.Join(ASSET_PATH, baseFolder), search));

            HashSet<string> results = new HashSet<string>();
            for (int ii = 0; ii < files.Count; ii++)
            {
                for (int jj = 0; jj < files[ii].Length; jj++)
                    results.Add(files[ii][jj]);
            }

            List<string> result_files = new List<string>();
            foreach (string str in results)
                result_files.Add(str);
            result_files.Sort();
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
