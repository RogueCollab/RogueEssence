using System;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using RogueEssence.Dev;

/*
 * MapBG.cs
 * 2021/02/06
 * idk
 * Description: An animated background for a map.
 */

namespace RogueEssence.Dungeon
{
    [Serializable]
    [JsonConverter(typeof(MapBGConverter))]
    public class MapBG : IBackgroundSprite
    {
        [Dev.SubGroup]
        public BGAnimData BGAnim;

        /// <summary>
        /// Pixels per Second
        /// </summary>
        public Loc BGMovement;

        /// <summary>
        /// 0f for no movement, 1f for movement in sync with map
        /// </summary>
        public Vector2 Parallax;

        public bool RepeatX;

        [Dev.SharedRow]
        public bool RepeatY;

        public Loc MapLoc { get; set; }
        public int LocHeight { get { return 0; } }

        public MapBG()
        {
            BGAnim = new BGAnimData();
        }
        public MapBG(BGAnimData anim, Loc movement)
        {
            BGAnim = anim;
            BGMovement = movement;
        }
        public MapBG(MapBG other)
        {
            BGAnim = new BGAnimData(other.BGAnim);
            BGMovement = other.BGMovement;
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }
        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (BGAnim.AnimIndex != "")
            {
                DirSheet sheet = GraphicsManager.GetBackground(BGAnim.AnimIndex);

                Loc movement = BGMovement * (int)FrameTick.TickToFrames(GraphicsManager.TotalFrameTick) / 60;
                Loc parallaxOffset = new Loc((int)(offset.X * Parallax.X), (int)(offset.Y * Parallax.Y));
                Loc diff = MapLoc + movement - parallaxOffset;
                float alpha = BGAnim.Alpha / 255f;
                if (sheet.Width == 1 && sheet.Height == 1)
                    sheet.DrawTile(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), 0, 0, Color.White * alpha);
                else
                {
                    if (RepeatX)
                    {
                        for (int x = diff.X % sheet.TileWidth - sheet.TileWidth; x < GraphicsManager.ScreenWidth; x += sheet.TileWidth)
                            drawTileY(spriteBatch, diff.Y, sheet, alpha, x);
                    }
                    else
                        drawTileY(spriteBatch, diff.Y, sheet, alpha, diff.X);
                }
            }
        }

        private void drawTileY(SpriteBatch spriteBatch, int diffY, DirSheet sheet, float alpha, int x)
        {
            if (RepeatY)
            {
                for (int y = diffY % sheet.TileHeight - sheet.TileHeight; y < GraphicsManager.ScreenHeight; y += sheet.TileHeight)
                    sheet.DrawDir(spriteBatch, new Vector2(x, y), BGAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), BGAnim.GetDrawDir(Dir8.None), Color.White * alpha);
            }
            else
                sheet.DrawDir(spriteBatch, new Vector2(x, diffY), BGAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), BGAnim.GetDrawDir(Dir8.None), Color.White * alpha);
        }


        public Loc GetDrawLoc(Loc offset)
        {
            return offset;
        }
        public Loc GetSheetOffset() { return Loc.Zero; }

        public Loc GetDrawSize()
        {
            return Loc.Zero;
        }
    }
}
