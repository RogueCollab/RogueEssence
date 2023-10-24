using System;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * GroundAnim.cs
 * 2017/07/03
 * idk
 * Description: An animated prop to be put on a GroundMap.  Unlike GroundObject, it cannot be collided or interacted with
 */

namespace RogueEssence.Ground
{
    [Serializable]
    public class GroundAnim : IDrawableSprite, IPreviewable
    {
        public IPlaceableAnimData ObjectAnim;
        
        public Loc MapLoc { get; set; }
        public int LocHeight { get { return 0; } }

        public GroundAnim()
        {
            ObjectAnim = new ObjAnimData();
            ObjectAnim.AnimDir = Dir8.Down;
        }
        public GroundAnim(IPlaceableAnimData anim, Loc loc)
        {
            ObjectAnim = anim;
            MapLoc = loc;
        }
        public GroundAnim(GroundAnim other)
        {
            ObjectAnim = (IPlaceableAnimData)other.ObjectAnim.Clone();
            MapLoc = other.MapLoc;
        }

        public void DrawDebug(SpriteBatch spriteBatch, Loc offset) { }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            DrawPreview(spriteBatch, offset, 1f);
        }

        public void DrawPreview(SpriteBatch spriteBatch, Loc offset, float alpha)
        {
            if (ObjectAnim.AnimIndex != "")
            {
                Loc drawLoc = GetDrawLoc(offset);

                DirSheet sheet = GraphicsManager.GetDirSheet(ObjectAnim.AssetType, ObjectAnim.AnimIndex);
                sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), ObjectAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), ObjectAnim.GetDrawDir(Dir8.None), Color.White * ((float)ObjectAnim.Alpha * alpha / 255), ObjectAnim.AnimFlip);
            }
        }


        public Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset;
        }
        public Loc GetSheetOffset() { return Loc.Zero; }

        public Loc GetDrawSize()
        {
            DirSheet sheet = GraphicsManager.GetObject(ObjectAnim.AnimIndex);

            return new Loc(sheet.TileWidth, sheet.TileHeight);
        }

        public Rect GetBounds()
        {
            Loc drawSize = GetDrawSize();
            return new Rect(MapLoc, new Loc(Math.Max(drawSize.X, GraphicsManager.TEX_SIZE), Math.Max(drawSize.Y, GraphicsManager.TEX_SIZE)));
        }
    }
}
