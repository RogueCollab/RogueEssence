using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class DevGraphicsManager
    {

        private static LRUCache<TileAddr, Bitmap> tileCache;
        private static LRUCache<string, Bitmap> tilesetCache;

        private static LRUCache<string, Dictionary<int, string>> aliasCache;

        public static Bitmap IconO;
        public static Bitmap IconX;

        public static List<CharSheetOp> CharSheetOps;

        public static void Init()
        {
            CharSheetOps = new List<CharSheetOp>();

            if (!Directory.Exists(Path.Combine(PathMod.RESOURCE_PATH, "Extensions")))
                Directory.CreateDirectory(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"));
            if (!Directory.Exists(Path.Combine(PathMod.RESOURCE_PATH, "UI")))
                Directory.CreateDirectory(Path.Combine(PathMod.RESOURCE_PATH, "UI"));

            foreach (string path in Directory.GetFiles(Path.Combine(PathMod.RESOURCE_PATH, "Extensions"), "*.op"))
            {
                try
                {
                    CharSheetOp newOp = Data.DataManager.LoadData<CharSheetOp>(path);
                    CharSheetOps.Add(newOp);
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }
            }


            IconO = new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "O.png"));
            IconX = new Bitmap(Path.Combine(PathMod.RESOURCE_PATH, "UI", "X.png"));

            tileCache = new LRUCache<TileAddr, Bitmap>(2000);
            tilesetCache = new LRUCache<string, Bitmap>(10);
            aliasCache = new LRUCache<string, Dictionary<int, string>>(20);
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
                Loc tileDims = GraphicsManager.TileIndex.GetTileDims(tileset);
                int tileSize = GraphicsManager.TileIndex.GetTileSize(tileset);

                System.Drawing.Bitmap baseBitmap = new System.Drawing.Bitmap(tileDims.X * tileSize, tileDims.Y * tileSize);

                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(baseBitmap))
                {
                    for (int xx = 0; xx < tileDims.X; xx++)
                    {
                        for (int yy = 0; yy < tileDims.Y; yy++)
                        {
                            long tilePos = GraphicsManager.TileIndex.GetPosition(tileset, new Loc(xx, yy));

                            if (tilePos > 0)
                            {
                                using (FileStream stream = new FileStream(PathMod.ModPath(String.Format(GraphicsManager.TILE_PATTERN, tileset)), FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    // Seek to the location of the tile
                                    stream.Seek(tilePos, SeekOrigin.Begin);

                                    using (BinaryReader reader = new BinaryReader(stream))
                                    {
                                        //usually handled by basesheet, cheat a little here
                                        long length = reader.ReadInt64();
                                        byte[] tileBytes = reader.ReadBytes((int)length);

                                        using (MemoryStream tileStream = new MemoryStream(tileBytes))
                                        {
                                            System.Drawing.Bitmap tile = new System.Drawing.Bitmap(tileStream);

                                            graphics.DrawImage(tile, xx * tileSize, yy * tileSize);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }



                using (MemoryStream tileStream = new MemoryStream())
                {
                    baseBitmap.Save(tileStream, System.Drawing.Imaging.ImageFormat.Png);
                    tileStream.Seek(0, SeekOrigin.Begin);
                    sheet = new Bitmap(tileStream);
                }
                tilesetCache.Add(tileset, sheet);
                return sheet;
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(new Exception("Error retrieving Tileset #" + tileset + "\n", ex));
            }
            return null;
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
