using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    /**
     * Autotile / tile importer for dungeon tiles in the Dungeon Tile Exchange Format.
     */
    public class DtefImportHelper
    {
        private const string VAR0_FN = "tileset_0.png";
        private const string VAR1_FN = "tileset_1.png";
        private const string VAR2_FN = "tileset_2.png";
        private static readonly Regex Var0FFn = new Regex(@"tileset_0_frame(\d+)_(\d+)\.(\d+)\.png");
        private static readonly Regex Var1FFn = new Regex(@"tileset_1_frame(\d+)_(\d+)\.(\d+)\.png");
        private static readonly Regex Var2FFn = new Regex(@"tileset_2_frame(\d+)_(\d+)\.(\d+)\.png");
        private const string XML_FN = "tileset.dtef.xml";
        private static readonly string[] VariantTitles = { VAR0_FN, VAR1_FN, VAR2_FN };
        private static readonly Regex[] VariantTitlesFrames = { Var0FFn, Var1FFn, Var2FFn };
        public static readonly string[] TileTitles = { "Wall", "Secondary", "Floor" };
        private const int MAX_VARIANTS = 3;

        /// This maps the internal tile IDs to the order in the DTEF templates:
        private static readonly int[] FieldDtefMapping =
        {
            0x89, 0x9B, 0x13, 0x09, 0x0A, 0x03,
            0xCD, 0xFF, 0x37, 0x05, 0x00, 0x06,
            0x4C, 0x6E, 0x26, 0x0C, 0x01, -1,
            0x3F, 0xCF, 0x0B, 0x08, 0x0F, 0x02,
            0x9F, 0x6F, 0x0E, 0x0D, 0x04, 0x07,
            0x7F, 0xEF, 0x4D, 0x27, 0x1B, 0x8B,
            0xBF, 0xDF, 0x8D, 0x17, 0x2E, 0x4E,
            0x8F, 0x1F, 0x4F, 0x2F, 0x5F, 0xAF
        };

        public static void ImportDtef(string sourceDir, string destFile)
        {
            string fileName = Path.GetFileName(sourceDir);
            int tileSize = -1;
            int tileTypes = -1;

            // Outer dict: Layer num; inner dict: frame num; tuple: (file name, frame length)
            var frameSpecs = new[] {
                    new SortedDictionary<int, SortedDictionary<int, Tuple<string, int>>>(),
                    new SortedDictionary<int, SortedDictionary<int, Tuple<string, int>>>(),
                    new SortedDictionary<int, SortedDictionary<int, Tuple<string, int>>>()
                };

            try
            {
                List<BaseSheet[]> tileList = new List<BaseSheet[]>();

                for (int vi = 0; vi < VariantTitles.Length; vi++)
                {
                    List<BaseSheet> tileArr = new List<BaseSheet>();
                    string variantFn = VariantTitles[vi];
                    Regex reg = VariantTitlesFrames[vi];
                    string path = Path.Join(sourceDir, variantFn);
                    if (!File.Exists(path))
                    {
                        if (variantFn == VAR0_FN)
                            throw new KeyNotFoundException($"Base variant missing for {fileName}.");
                        continue;
                    }

                    // Import main frame
                    BaseSheet tileset = BaseSheet.Import(path);
                    int newTileSize = tileset.Height / 8;
                    if (tileSize > 0 && newTileSize != tileSize)
                        throw new InvalidDataException($"Bad dimensions for {fileName}.");
                    tileSize = newTileSize;

                    int newTileTypes = tileset.Width / tileSize / 6;
                    if (tileTypes > 0 && newTileTypes != tileTypes)
                        throw new InvalidDataException($"Bad dimensions for {fileName}.");
                    tileTypes = newTileTypes;
                    tileArr.Add(tileset);

                    // List additional layers and their frames - We do it this way in two steps to make sure it's sorted
                    foreach (var frameFn in Directory.GetFiles(sourceDir, "*.png"))
                    {
                        if (!reg.IsMatch(frameFn))
                            continue;
                        Match match = reg.Match(frameFn);
                        int layerIdx = int.Parse(match.Groups[1].ToString());
                        int frameIdx = int.Parse(match.Groups[2].ToString());
                        int durationIdx = int.Parse(match.Groups[3].ToString());
                        if (!frameSpecs[vi].ContainsKey(layerIdx))
                            frameSpecs[vi].Add(layerIdx, new SortedDictionary<int, Tuple<string, int>>());
                        // GetFiles lists some files twice??
                        if (!frameSpecs[vi][layerIdx].ContainsKey(frameIdx))
                            frameSpecs[vi][layerIdx].Add(frameIdx, new Tuple<string, int>(frameFn, durationIdx));
                    }

                    // Import additional frames
                    foreach (var layerFn in frameSpecs[vi].Values)
                    {
                        foreach (var frameFn in layerFn.Values)
                        {
                            // Import frame 
                            tileset = BaseSheet.Import(frameFn.Item1);
                            tileArr.Add(tileset);
                        }
                    }
                    tileList.Add(tileArr.ToArray());
                }

                for (int tt = 0; tt < tileTypes; tt++)
                {
                    var tileTitle = TileTitles[tt];
                    bool hasTiles = false;

                    AutoTileData autoTile = new AutoTileData();
                    AutoTileAdjacent entry = new AutoTileAdjacent();

                    List<TileLayer>[][] totalArray = new List<TileLayer>[48][];

                    for (int jj = 0; jj < FieldDtefMapping.Length; jj++)
                    {
                        totalArray[jj] = new List<TileLayer>[tileList.Count];
                        for (int kk = 0; kk < tileList.Count; kk++)
                            totalArray[jj][kk] = new List<TileLayer>();
                    }

                    for (int jj = 0; jj < FieldDtefMapping.Length; jj++)
                    {
                        for (int kk = 0; kk < tileList.Count; kk++)
                        {
                            if (FieldDtefMapping[jj] == -1)
                                continue; // Skip empty tile

                            int tileX = 6 * tt + jj % 6;
                            int tileY = jj / 6;

                            // Base Layer
                            TileLayer baseLayer = new TileLayer { FrameLength = 999 };
                            BaseSheet tileset = tileList[kk][0];
                            //keep adding more tiles to the anim until end of blank spot is found
                            if (!tileset.IsBlank(tileX * tileSize, tileY * tileSize, tileSize, tileSize))
                                baseLayer.Frames.Add(new TileFrame(new Loc(tileX + kk * 6 * tileTypes, tileY), fileName));
                            if (baseLayer.Frames.Count < 1)
                                continue;
                            totalArray[jj][kk].Add(baseLayer);
                            hasTiles = true;

                            // Additional layers
                            int curFrame = 1;
                            foreach (var layer in frameSpecs[kk].Values)
                            {
                                if (layer.Count < 1)
                                    continue;
                                TileLayer anim = new TileLayer { FrameLength = layer[0].Item2 };

                                for (int mm = 0; mm < layer.Count; mm++)
                                {
                                    tileset = tileList[kk][curFrame];
                                    //keep adding more tiles to the anim until end of blank spot is found
                                    if (!tileset.IsBlank(tileX * tileSize, tileY * tileSize, tileSize, tileSize))
                                        anim.Frames.Add(new TileFrame(new Loc(tileX + kk * 6 * tileTypes, tileY + curFrame * 8), fileName));

                                    curFrame += 1;
                                }

                                if (anim.Frames.Count > 0)
                                    totalArray[jj][kk].Add(anim);
                            }
                        }
                    }

                    if (!hasTiles)
                        continue;

                    // Import auto tiles
                    for (int ii = 0; ii < FieldDtefMapping.Length; ii++)
                    {
                        if (FieldDtefMapping[ii] == -1)
                            continue;
                        List<List<TileLayer>> tileArray = typeof(AutoTileAdjacent)
                            .GetField($"Tilex{FieldDtefMapping[ii]:X2}")
                            .GetValue(entry) as List<List<TileLayer>>;
                        ImportTileVariant(tileArray, totalArray[ii]);
                    }

                    autoTile.Tiles = entry;

                    if (tileTypes > 1)
                        autoTile.Name = new LocalText(Text.GetMemberTitle(fileName) + " " + tileTitle);
                    else
                        autoTile.Name = new LocalText(Text.GetMemberTitle(fileName));

                    string index = Text.Sanitize(autoTile.Name.DefaultText).ToLower();
                    DataManager.SaveEntryData(index, DataManager.DataType.AutoTile.ToString(), autoTile);
                    Debug.WriteLine($"{index}: {autoTile.Name}");
                }
                ImportHelper.SaveTileSheet(tileList, destFile, tileSize);
                foreach (BaseSheet[] arr in tileList)
                    foreach(BaseSheet tex in arr)
                        tex.Dispose();
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error importing " + fileName + "\n", ex));
            }
        }

        public static void ExportDtefTile(int index, string outputPath)
        {
            //TODO: export just the tiles relevant to the particular autotile type
        }

        public static void ImportAllDtefTiles(string sourceDir, string cachePattern)
        {
            string[] dirs = Directory.GetDirectories(sourceDir);
            foreach (string dir in dirs)
            {
                string fileName = Path.GetFileName(dir);
                string outputFnTiles = string.Format(cachePattern, fileName);
                DiagManager.Instance.LoadMsg = "Importing " + fileName;
                ImportDtef(dir, outputFnTiles);
            }
        }

        private static void ImportTileVariant(List<List<TileLayer>> list, List<TileLayer>[] data)
        {
            //add the variant to the appropriate entry list
            for (var kk = 0; kk < data.Length; kk++)
            {
                if (data[kk].Count > 0)
                    list.Add(data[kk]);
            }
        }
    }
}