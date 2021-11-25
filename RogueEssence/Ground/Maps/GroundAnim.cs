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
    public class GroundAnim : IDrawableSprite
    {
        public IPlaceableAnimData ObjectAnim;
        
        public Loc MapLoc { get; set; }
        public int LocHeight { get { return 0; } }

        public GroundAnim()
        {
            ObjectAnim = new ObjAnimData();
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
            if (ObjectAnim.AnimIndex != "")
            {
                Loc drawLoc = GetDrawLoc(offset);

                DirSheet sheet = GraphicsManager.GetDirSheet(ObjectAnim.AssetType, ObjectAnim.AnimIndex);
                sheet.DrawDir(spriteBatch, drawLoc.ToVector2(), ObjectAnim.GetCurrentFrame(GraphicsManager.TotalFrameTick, sheet.TotalFrames), ObjectAnim.GetDrawDir(Dir8.None), Color.White * ((float)ObjectAnim.Alpha / 255), ObjectAnim.AnimFlip);
            }
        }


        public Loc GetDrawLoc(Loc offset)
        {
            return MapLoc - offset;
        }

        public Loc GetDrawSize()
        {
            DirSheet sheet = GraphicsManager.GetObject(ObjectAnim.AnimIndex);

            return new Loc(sheet.TileWidth, sheet.TileHeight);
        }
    }
}
