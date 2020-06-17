using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Content
{
    public class Emote : BaseAnim
    {
        public Emote() { }
        public Emote(AnimData anim, int height, int cycles)
        {
            Anim = anim;
            locHeight = height;
            TotalTime = anim.FrameTime * anim.GetTotalFrames(GraphicsManager.GetAttackSheet(anim.AnimIndex).TotalFrames) * cycles;
        }

        protected AnimData Anim;

        protected int TotalTime;
        
        protected Dir8 Direction;

        protected int Frame;

        private FrameTick ActionTime;
        
        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;

            if (TotalTime > 0 && ActionTime >= TotalTime)
                finished = true;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            Loc drawLoc = GetDrawLoc(offset);

            DirSheet sheet = GraphicsManager.GetAttackSheet(Anim.AnimIndex);
            sheet.DrawDir(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y - LocHeight), Anim.GetCurrentFrame(ActionTime, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TotalFrames), DirExt.AddAngles(Direction, Anim.AnimDir), Color.White * ((float)Anim.Alpha / 255));

        }

        public override Loc GetDrawLoc(Loc offset)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileHeight / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileHeight);
        }

    }
}