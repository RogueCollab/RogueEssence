using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Dev;
using System.Runtime.Serialization;

namespace RogueEssence.Content
{
    [Serializable]
    public abstract class LoopingAnim : BaseAnim
    {
        public LoopingAnim() { Anim = new AnimData(); }
        public LoopingAnim(AnimData anim) { Anim = anim; }
        public LoopingAnim(AnimData anim, int totalTime)
        {
            Anim = anim;
            TotalTime = totalTime;
        }
        public LoopingAnim(AnimData anim, int totalTime, int cycles)
        {
            Anim = anim;
            TotalTime = totalTime;
            Cycles = cycles;
        }

        protected LoopingAnim(LoopingAnim other)
        {
            Anim = other.Anim;
            TotalTime = other.TotalTime;
            Cycles = other.Cycles;
            FrameOffset = other.FrameOffset;
        }

        [SubGroup]
        public AnimData Anim;

        /// <summary>
        /// In frames
        /// </summary>
        public int TotalTime;
        public int Cycles;

        /// <summary>
        /// Time difference to start animating at
        /// </summary>
        public int FrameOffset;

        [NonSerialized]
        public Dir8 Direction;

        [NonSerialized]
        protected FrameTick ActionTime;

        public void SetUp()
        {
            TotalTime = Cycles > 0 ? Anim.FrameTime * Anim.GetTotalFrames(GraphicsManager.GetAttackSheet(Anim.AnimIndex).TotalFrames) * Math.Max(1, Cycles) : TotalTime;
        }

        public override void Update(BaseScene scene, FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;

            if (ActionTime >= TotalTime)
                finished = true;
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            if (Finished)
                return;

            Loc drawLoc = GetDrawLoc(offset);

            DirSheet sheet = GraphicsManager.GetAttackSheet(Anim.AnimIndex);
            sheet.DrawDir(spriteBatch, new Vector2(drawLoc.X, drawLoc.Y - LocHeight), Anim.GetCurrentFrame(ActionTime + FrameOffset, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TotalFrames), Anim.GetDrawDir(Direction), Color.White * ((float)Anim.Alpha / 255), Anim.AnimFlip);

        }

        public override Loc GetDrawLoc(Loc offset)//TODO: transfer this offset out of the draw call and into the call of whatever emitter created it (or the creator of that emitter)
        {
            return new Loc(MapLoc.X - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth / 2,
                MapLoc.Y - GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileHeight / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileWidth, GraphicsManager.GetAttackSheet(Anim.AnimIndex).TileHeight);
        }



        public override string ToString()
        {
            return Anim.ToString();
        }
    }
}