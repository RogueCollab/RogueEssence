using System;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RogueEssence.Content
{
    public class MultiAnimSprite
    {
        public MultiAnimSprite(string animIndex, int frame, Loc position, int height)
        {
            AnimIndex = animIndex;
            Frame = frame;
            Position = position;
            Height = height;
            Alpha = 255;
        }
        public MultiAnimSprite(string animIndex, int frame, Loc position, int height, Dir8 dir, byte alpha)
        {
            AnimIndex = animIndex;
            Frame = frame;
            Position = position;
            Height = height;
            AnimDir = dir;
            Alpha = alpha;
        }

        public MultiAnimSprite(MultiAnimSprite other)
        {
            AnimIndex = other.AnimIndex;
            Frame = other.Frame;
            Position = other.Position;
            Height = other.Height;
        }

        public string AnimIndex;
        public int Frame;
        public Loc Position;
        public int Height;
        public Dir8 AnimDir;
        public byte Alpha;
    }


    public class MultiAnimFrame
    {
        public MultiAnimFrame()
        {
            Sprites = new List<MultiAnimSprite>();
        }
        public MultiAnimFrame(int totalTime, params MultiAnimSprite[] sprites)
        {
            TotalTime = totalTime;
            Sprites = new List<MultiAnimSprite>();
            Sprites.AddRange(sprites);
        }
        protected MultiAnimFrame(MultiAnimFrame other)
        {
            TotalTime = other.TotalTime;
            Sprites = new List<MultiAnimSprite>();
            foreach (MultiAnimSprite sprite in other.Sprites)
                Sprites.Add(new MultiAnimSprite(sprite));
        }
        public MultiAnimFrame Clone() { return new MultiAnimFrame(this); }

        public int TotalTime { get; protected set; }

        protected List<MultiAnimSprite> Sprites;

        public Rect GetFrameRect()
        {
            Rect rect = new Rect();

            foreach (MultiAnimSprite sprite in Sprites)
                rect = Rect.Union(rect, new Rect(sprite.Position, new Loc(GraphicsManager.GetAttackSheet(sprite.AnimIndex).Width, GraphicsManager.GetAttackSheet(sprite.AnimIndex).Height)));

            return rect;
        }

        public void Draw(SpriteBatch spriteBatch, Loc parentLoc, Dir8 dir, int locHeight, float sizeRatio)
        {
            foreach (MultiAnimSprite sprite in Sprites)
            {
                DirSheet sheet = GraphicsManager.GetAttackSheet(sprite.AnimIndex);
                sheet.DrawDir(spriteBatch, new Vector2(parentLoc.X, parentLoc.Y - locHeight) + sprite.Position.ToVector2() * sizeRatio, sprite.Frame, DirExt.AddAngles(dir, sprite.AnimDir), Color.White * ((float)sprite.Alpha / 255));
            }
        }
    }

    public class MultiAnim : BaseAnim
    {
        public MultiAnim()
        {
            SizeRatio = 1f;
            Frames = new List<MultiAnimFrame>();
        }
        protected MultiAnim(MultiAnim other)
        {
            Frames = new List<MultiAnimFrame>();
            foreach (MultiAnimFrame frame in other.Frames)
                Frames.Add(frame.Clone());
        }
        //creates a multianim for "live" use
        //TODO: find a better pattern for this
        public MultiAnim(MultiAnim other, float sizeRatio, Loc mapLoc, Dir8 dir, int locHeight, int cycles)
        {
            Frames = new List<MultiAnimFrame>();
            //clone all main attributes
            AnimSize = new Rect();
            foreach (MultiAnimFrame frame in other.Frames)
            {
                Frames.Add(frame.Clone());
                //calculate animSize
                AnimSize = Rect.Union(AnimSize, frame.GetFrameRect());
                TotalFrameTime += frame.TotalTime;
            }
            //adopt sizeRatio
            SizeRatio = sizeRatio;

            this.mapLoc = mapLoc;
            Direction = dir;
            this.locHeight = locHeight;
            TotalTime = TotalFrameTime * Math.Max(1, cycles);
        }
        public MultiAnim Clone() { return new MultiAnim(this); }

        protected List<MultiAnimFrame> Frames;




        public int TotalTime;

        public float SizeRatio;

        public Rect AnimSize;
        public int TotalFrameTime;

        public Dir8 Direction;
        private FrameTick ActionTime;


        private int getCurrentFrame(FrameTick time)
        {
            int countingFrameTicks = 0;
            int curFrames = time.ToFrames() % TotalFrameTime;
            for (int ii = 0; ii < Frames.Count; ii++)
            {
                countingFrameTicks += Frames[ii].TotalTime;
                if (curFrames < countingFrameTicks)
                    return ii;
            }
            return 0;
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

            int frame = getCurrentFrame(ActionTime);
            Frames[frame].Draw(spriteBatch, drawLoc, Direction, LocHeight, SizeRatio);
        }

        public override Loc GetDrawLoc(Loc offset)//TODO: transfer this offset out of the draw call and into the call of whatever emitter created it (or the creator of that emitter)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - AnimSize.X / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - AnimSize.Y / 2) - offset;
        }

        public override Loc GetDrawSize()
        {
            return new Loc(AnimSize.X, AnimSize.Y);
        }

    }
}