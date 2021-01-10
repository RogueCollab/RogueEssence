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
        private static readonly Regex Var0FFn = new Regex(@"tileset_0_frame(\d+)\.(\d+)\.png");
        private static readonly Regex Var1FFn = new Regex(@"tileset_1_frame(\d+)\.(\d+)\.png");
        private static readonly Regex Var2FFn = new Regex(@"tileset_2_frame(\d+)\.(\d+)\.png");
        private const string XML_FN = "tileset.dtef.xml";
        private static readonly string[] VariantTitles = { VAR0_FN, VAR1_FN, VAR2_FN };
        private static readonly Regex[] VariantTitlesFrames = { Var0FFn, Var1FFn, Var2FFn };
        private static readonly string[] TileTitles = { "Wall", "Floor", "Secondary" };
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

        public static void ImportAllDtefTiles(string sourceDir, string cachePattern)
        {
            var dirs = Directory.GetDirectories(sourceDir);
            foreach (var dir in dirs)
            {
                var fileName = Path.GetFileName(dir);
                var outputFnTiles = string.Format(cachePattern, fileName);
                DiagManager.Instance.LoadMsg = "Importing " + fileName;

                // Read XML for layer mapping
                var document = new XmlDocument();
                document.Load(Path.Join(dir, XML_FN));
                var tileSize = int.Parse(document.DocumentElement.GetAttribute("dimensions"));

                // The tile index inside the tile sheet where the first frame of animation for this variation is.
                var variationStarts = new[] {0, 0, 0};
                var frameCounts = new[] {0, 0, 0};

                try
                {
                    var tileList = new List<BaseSheet>();
                    foreach (var tileTitle in TileTitles)
                    {
                        var frameLength = -1;
                        for (var i = 0; i < VariantTitles.Length; i++)
                        {
                            variationStarts[i] = tileList.Count;
                            var variantFn = VariantTitles[i];
                            var reg = VariantTitlesFrames[i];
                            var path = Path.Join(dir, variantFn);
                            if (!File.Exists(path))
                            {
                                if (variantFn != VAR0_FN)
                                    throw new KeyNotFoundException($"Base variant missing for {fileName}.");
                                continue;
                            }
                            
                            // Import main frame
                            var tileset = BaseSheet.Import(path);
                            tileList.Add(tileset);
                            
                            // List additional frames - We do it this way in two steps to make sure it's sorted
                            var frameSpecs = new SortedDictionary<int, string>();
                            foreach (var frameFn in Directory.GetFiles(dir, "*.png"))
                            {
                                if (!reg.IsMatch(frameFn))
                                    continue;
                                var match = reg.Match(frameFn);
                                frameSpecs.Add(int.Parse(match.Groups[1].ToString()), frameFn);
                                // We take the first speed as the frame length
                                if (frameLength == -1)
                                    frameLength = int.Parse(match.Groups[2].ToString());
                            }

                            frameCounts[i] = frameSpecs.Count;
                            // Import additional frames
                            foreach (var frameFn in frameSpecs.Values)
                            {
                                // Import frame 
                                tileset = BaseSheet.Import(frameFn);
                                tileList.Add(tileset);
                            }

                        }
                        var node = document.SelectSingleNode("//DungeonTileset/PMDO/" + tileTitle);
                        var index = -1;
                        if (node != null)
                            index = int.Parse(node.InnerText);
                        if (frameLength == -1)
                            frameLength = 60;

                        
                        var autoTile = new AutoTileData();
                        var entry = new AutoTileAdjacent();

                        var totalArray = new List<TileLayer>[48][];

                        for (var jj = 0; jj < FieldDtefMapping.Length; jj++)
                        {
                            totalArray[jj] = new List<TileLayer>[3];
                            for (var kk = 0; kk < MAX_VARIANTS; kk++)
                                totalArray[jj][kk] = new List<TileLayer>();
                        }

                        for (var jj = 0; jj < FieldDtefMapping.Length; jj++)
                        {
                            for (var kk = 0; kk < MAX_VARIANTS; kk++)
                            {
                                if (FieldDtefMapping[jj] == -1)
                                    continue; // Skip empty tile
                                
                                //go through each layer
                                var anim = new TileLayer {FrameLength = frameLength};

                                for (var mm = 0; mm < frameCounts[kk]; mm++)
                                {
                                    var idx = variationStarts[kk] + mm;
                                    if (tileList.Count <= idx)
                                        continue;
                                    var tileset = tileList[idx];
                                    //keep adding more tiles to the anim until end of blank spot is found
                                    var offIndex = tileTitle switch
                                    {
                                        "Secondary" => 1,
                                        "Floor" => 2,
                                        _ => 0
                                    };
                                    var tileX = 6 * offIndex + jj % 6;
                                    var tileY = (int) Math.Floor(jj / 6.0);
                                    if (!tileset.IsBlank(tileX * tileSize, tileY * tileSize, tileSize, tileSize))
                                        anim.Frames.Add(new TileFrame(new Loc(tileX, tileY + idx * 8), fileName));
                                }

                                if (anim.Frames.Count > 0)
                                    totalArray[jj][kk].Add(anim);
                            }
                        }

                        if (index == -1)
                        {
                            if (tileTitle == "Secondary")  // Secondary terrain is okay to be missing.
                                continue;
                            throw new KeyNotFoundException($"Layer index mapping for layer {tileTitle} for {fileName} missing.");
                        }
                        
                        // Import auto tiles
                        for (var i = 0; i < FieldDtefMapping.Length; i++)
                        {
                            if (FieldDtefMapping[i] == -1)
                                continue;
                            List<List<TileLayer>> tileArray = typeof(AutoTileAdjacent)
                                .GetField($"Tilex{FieldDtefMapping[i]:X2}")
                                .GetValue(entry) as List<List<TileLayer>>;
                            ImportTileVariant(tileArray, totalArray[i]);
                        }

                        autoTile.Tiles = entry;

                        autoTile.Name = new LocalText(fileName + tileTitle);

                        DataManager.SaveData(index, DataManager.DataType.AutoTile.ToString(), autoTile);
                        Debug.WriteLine($"{index:D3}: {autoTile.Name}");
                    }
                    ImportHelper.SaveTileSheet(tileList, outputFnTiles, tileSize);
                    foreach (var tex in tileList)
                        tex.Dispose();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(new Exception("Error importing " + fileName + "\n", ex));
                }
            }
        }

        private static void ImportTileVariant(List<List<TileLayer>> list, List<TileLayer>[] data)
        {
            //add the variant to the appropriate entry list
            for (var kk = 0; kk < MAX_VARIANTS; kk++)
            {
                if (data[kk].Count > 0)
                    list.Add(data[kk]);
            }
        }
    }
}