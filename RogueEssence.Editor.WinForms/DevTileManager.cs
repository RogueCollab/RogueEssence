using System;
using System.Drawing;
using System.IO;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Dev
{
    public class DevTileManager
    {
        private static DevTileManager instance;

        public static DevTileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new DevTileManager();
                return instance;
            }
        }

        LRUCache<TileFrame, Bitmap> tileCache;

        private DevTileManager()
        {
            tileCache = new LRUCache<TileFrame, Bitmap>(2000);
        }

        public Bitmap GetTile(TileFrame tileTex)
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

    }
}
