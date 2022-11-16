using System;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using RogueEssence.Dev;

/*
 * TileBG.cs
 * 2021/02/06
 * idk
 * Description: An animated background for a map, based on tile.
 */

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class TileBG : IBackgroundSprite
    {
        [Dev.SubGroup]
        public AutoTile Tile;

        public Loc MapLoc { get { return Loc.Zero; } }
        public int LocHeight { get { return 0; } }

        public TileBG()
        {
            Tile = new AutoTile();
        }
        public TileBG(AutoTile tile)
        {
            Tile = tile;
        }
        public TileBG(TileBG other)
        {
            Tile = other.Tile.Copy();
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            for (int x = -offset.X % GraphicsManager.TileSize - GraphicsManager.TileSize; x < GraphicsManager.ScreenWidth; x += GraphicsManager.TileSize)
            {
                for (int y = -offset.Y % GraphicsManager.TileSize - GraphicsManager.TileSize; y < GraphicsManager.ScreenHeight; y += GraphicsManager.TileSize)
                    Tile.DrawBlank(spriteBatch, new Loc(x, y), ulong.MaxValue);
            }
        }

        public Loc GetDrawLoc(Loc offset)
        {
            return offset;
        }

        public Loc GetDrawSize()
        {
            return Loc.Zero;
        }
    }
}
