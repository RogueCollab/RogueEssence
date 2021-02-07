using System;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
/*
 * MapBG.cs
 * 2021/02/06
 * idk
 * Description: An animated background for a map.
 */

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class MapBG : IDrawableSprite
    {
        public BGAnimData BGAnim;
        public Loc BGMovement;

        public Loc MapLoc { get { return Loc.Zero; } }
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

                Loc diff = BGMovement * (int)FrameTick.TickToFrames(GraphicsManager.TotalFrameTick) / 60;
                if (sheet.Width == 1 && sheet.Height == 1)
                    sheet.DrawTile(spriteBatch, new Rectangle(0, 0, GraphicsManager.ScreenWidth, GraphicsManager.ScreenHeight), 0, 0, Color.White);
                else
                {
                    for (int x = diff.X % sheet.TileWidth - sheet.TileWidth; x < GraphicsManager.ScreenWidth; x += sheet.TileWidth)
                    {
                        for (int y = diff.Y % sheet.TileHeight - sheet.TileHeight; y < GraphicsManager.ScreenHeight; y += sheet.TileHeight)
                            sheet.DrawDir(spriteBatch, new Vector2(x, y), BGAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), BGAnim.AnimDir, Color.White);
                    }
                }
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
