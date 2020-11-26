using System;
using System.IO;
using Avalonia.Media.Imaging;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class DevTileManager
    {
        public const string RESOURCE_PATH = DiagManager.CONTENT_PATH + "Editor/";

        private static LRUCache<TileFrame, Bitmap> tileCache;
        private static LRUCache<string, Bitmap> tilesetCache;

        public static Bitmap IconO;
        public static Bitmap IconX;

        public static void Init()
        {
            IconO = new Bitmap(Path.Join(RESOURCE_PATH, "O.png"));
            IconX = new Bitmap(Path.Join(RESOURCE_PATH, "X.png"));

            tileCache = new LRUCache<TileFrame, Bitmap>(2000);
            tilesetCache = new LRUCache<string, Bitmap>(10);
        }

        public static Bitmap GetTile(TileFrame tileTex)
        {
            Bitmap sheet;
            if (tileCache.TryGetValue(tileTex, out sheet))
                return sheet;

            long tilePos = GraphicsManager.TileIndex.GetPosition(tileTex.Sheet, tileTex.TexLoc);

            if (tilePos > 0)
            {
                try
                {
                    using (FileStream stream = new FileStream(String.Format(GraphicsManager.TILE_PATTERN, tileTex.Sheet), FileMode.Open, FileAccess.Read, FileShare.Read))
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

                    tileCache.Add(tileTex, sheet);
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
                                using (FileStream stream = new FileStream(String.Format(GraphicsManager.TILE_PATTERN, tileset), FileMode.Open, FileAccess.Read, FileShare.Read))
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
    }
}
