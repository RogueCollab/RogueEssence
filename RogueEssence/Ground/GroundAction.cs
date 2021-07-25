using System;
using System.Collections.Generic;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Ground
{

    [Serializable]
    public abstract class GroundAction
    {
        public const int HITBOX_WIDTH = 16;
        public const int HITBOX_HEIGHT = 16;
        protected abstract int AnimFrameType { get; }
        protected virtual int FrameMethod(List<CharAnimFrame> frames) { return zeroFrame(frames); }

        public Loc MapLoc { get { return collider.Start; } set { collider.Start = value; } }
        public Loc Move { get; protected set; }

        protected Rect collider;
        public Rect Collider { get { return collider; } }
        public int LocHeight { get; set; }
        public Dir8 CharDir { get; set; }

        public FrameTick ActionTime { get; protected set; }

        protected Dir8 dirOffset;
        protected int opacity;
        public Loc DrawOffset { get; protected set; }


        public int AnimRushTime { get; private set; }
        public int AnimHitTime { get; private set; }
        public int AnimReturnTime { get; private set; }
        public int AnimTotalTime { get; private set; }

        public GroundAction NextAction;

        public GroundAction()
        {
            collider = new Rect(0, 0, HITBOX_WIDTH, HITBOX_HEIGHT);
        }

        public virtual void UpdateInput(GameAction action) { }
        public void Begin(MonsterID appearance)
        {
            AnimRushTime = GraphicsManager.GetChara(appearance).GetRushTime(AnimFrameType, CharDir);
            AnimHitTime = GraphicsManager.GetChara(appearance).GetHitTime(AnimFrameType, CharDir);
            AnimReturnTime = GraphicsManager.GetChara(appearance).GetReturnTime(AnimFrameType, CharDir);
            AnimTotalTime = GraphicsManager.GetChara(appearance).GetTotalTime(AnimFrameType, CharDir);
        }

        public virtual void Update(FrameTick elapsedTime) { }
        public virtual void UpdateTime(FrameTick elapsedTime)
        {
            ActionTime += elapsedTime;
        }

        protected virtual void UpdateFrameInternal() { }
        public void UpdateFrame()
        {
            DrawOffset = new Loc();
            dirOffset = Dir8.Down;
            opacity = 255;

            UpdateFrameInternal();
        }


        public virtual void Draw(SpriteBatch spriteBatch, Loc offset, CharSheet sheet)
        {
            Loc drawLoc = GetDrawLoc(offset, sheet);
            drawLoc.Y -= LocHeight;
            //draw sprite at current frame
            sheet.DrawChar(spriteBatch, AnimFrameType, true, DirExt.AddAngles(CharDir, dirOffset), drawLoc.ToVector2(), FrameMethod, Microsoft.Xna.Framework.Color.White * ((float)opacity / 255));
        }
        public virtual Loc GetActionPoint(CharSheet sheet, ActionPointType pointType)
        {
            return MapLoc + Collider.Size / 2 +  DrawOffset + sheet.GetActionPoint(AnimFrameType, true, DirExt.AddAngles(CharDir, dirOffset), pointType, FrameMethod);
        }

        private int zeroFrame(List<CharAnimFrame> frames)
        {
            return 0;
        }

        public Loc GetDrawLoc(Loc offset, CharSheet sheet)
        {
            return new Loc(MapLoc.X + Collider.Width / 2 - sheet.TileWidth / 2,
                MapLoc.Y + Collider.Height / 2 - sheet.TileHeight / 2) + DrawOffset - offset;
        }

        public void GetCurrentSprite(CharSheet sheet, out int currentAnim, out int currentTime, out int currentFrame)
        {
            currentAnim = AnimFrameType;
            currentTime = ActionTime.ToFrames();
            currentFrame = sheet.GetCurrentFrame(AnimFrameType, DirExt.AddAngles(CharDir, dirOffset), FrameMethod);
        }
    }


    [Serializable]
    public class IdleGroundAction : GroundAction
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        protected override int AnimFrameType { get { return GraphicsManager.GlobalIdle; } }
        public IdleGroundAction(Loc loc, Dir8 dir)
        {
            MapLoc = loc;
            CharDir = dir;
        }

        public override void UpdateInput(GameAction action)
        {
            //trigger walk if ordered to
            if (action.Type == GameAction.ActionType.Move)
                NextAction = new WalkGroundAction(MapLoc, action.Dir, action[0] != 0, new FrameTick());
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset, CharSheet sheet)
        {
            Loc drawLoc = GetDrawLoc(offset, sheet);
            drawLoc.Y -= LocHeight;
            //draw sprite at current frame
            sheet.DrawChar(spriteBatch, AnimFrameType, false, DirExt.AddAngles(CharDir, dirOffset), drawLoc.ToVector2(), FrameMethod, Microsoft.Xna.Framework.Color.White * ((float)opacity / 255));
        }
        public override Loc GetActionPoint(CharSheet sheet, ActionPointType pointType)
        {
            return MapLoc + Collider.Size / 2 + DrawOffset + sheet.GetActionPoint(AnimFrameType, false, DirExt.AddAngles(CharDir, dirOffset), pointType, FrameMethod);
        }
    }

    [Serializable]
    public class WalkGroundAction : GroundAction
    {
        private bool run;
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        protected override int AnimFrameType { get { return GraphicsManager.WalkAction; } }
        public WalkGroundAction(Loc loc, Dir8 dir, bool run, FrameTick prevTime)
        {
            MapLoc = loc;
            CharDir = dir;
            this.run = run;
            ActionTime = prevTime;
        }

        public override void UpdateTime(FrameTick elapsedTime)
        {
            base.UpdateTime(elapsedTime * (run ? 2 : 1));
        }

        public override void UpdateInput(GameAction action)
        {
            if (action.Type == GameAction.ActionType.Move)//change direction of walk if ordered to
            {
                CharDir = action.Dir;
                run = (action[0] != 0);
            }
            else if (action.Type == GameAction.ActionType.None)//start skid if ordered to
                NextAction = new IdleGroundAction(MapLoc, CharDir);// new SkidGroundAction(MapLoc, CharDir, ActionTime);
            // attempting to interact does not halt movement
            // but we don't want players to notice
            // so don't change direction or run state until the frame after
            // note that this only applies to interaction attempts that fail to find something to interact with.
        }

        public override void Update(FrameTick elapsedTime)
        {
            //set the character's projected movement
            Move = CharDir.GetLoc() * (run ? 5 : 2);
        }
    }

    [Serializable]
    public class IdleNoAnim : GroundAction
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, 0, false); //Don't animate
        }
        protected override int AnimFrameType { get { return GraphicsManager.GlobalIdle; } }
        public IdleNoAnim(Loc loc, Dir8 dir)
        {
            MapLoc = loc;
            CharDir = dir;
        }

        public override void UpdateInput(GameAction action)
        {
            //trigger walk if ordered to
            if (action.Type == GameAction.ActionType.Move)
                NextAction = new WalkGroundAction(MapLoc, action.Dir, action[0] != 0, new FrameTick());
        }
    }


    [Serializable]
    public class SkidGroundAction : GroundAction
    {
        private FrameTick skidTime;

        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        protected override int AnimFrameType { get { return GraphicsManager.WalkAction; } }
        public SkidGroundAction(Loc loc, Dir8 dir, FrameTick prevTime)
        {
            MapLoc = loc;
            CharDir = dir;
            ActionTime = prevTime;
            skidTime = prevTime;
        }


        public override void UpdateTime(FrameTick elapsedTime)
        {
            base.UpdateTime(elapsedTime);
        }

        public override void UpdateInput(GameAction action)
        {
            if (action.Type == GameAction.ActionType.Move)//start walk if ordered to
                NextAction = new WalkGroundAction(MapLoc, action.Dir, action[0] != 0, ActionTime);
            else
            {
                int prevTime = (skidTime / AnimTotalTime).ToFrames();
                int newTime = (ActionTime / AnimTotalTime).ToFrames();
                if (prevTime < newTime)//stop skid if timed out
                    NextAction = new IdleGroundAction(MapLoc, CharDir);
            }
        }

    }



    //
    // Generic Actions
    //

    /// <summary>
    /// A generic GroundAction that makes a groundchar play an animation and loops forever.
    ///
    /// NOTE: I just made this as a concept class to demonstrate how we could allow script users to run animations on demand on groundchars.
    /// </summary>
    [Serializable]
    public class IdleAnimGroundAction : GroundAction
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, !Loop);
        }
        protected override int AnimFrameType { get { return AnimID; } }

        int AnimID;
        bool Loop;

        public IdleAnimGroundAction(Loc pos, Dir8 dir, int animid, bool loop)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
            Loop = loop;
        }

        public override void UpdateInput(GameAction action)
        {
            if (!Loop && ActionTime >= AnimTotalTime)
                NextAction = new IdleGroundAction(MapLoc, CharDir);
        }
    }

    [Serializable]
    public class PoseGroundAction : GroundAction
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, Math.Min(ActionTime.Ticks, FrameTick.FrameToTick(AnimReturnTime)), true);
        }
        protected override int AnimFrameType { get { return AnimID; } }

        int AnimID;

        public PoseGroundAction(Loc pos, Dir8 dir, int animid)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
        }

        public override void UpdateInput(GameAction action)
        {

        }
    }

    [Serializable]
    public class WalkToPositionGroundAction : GroundAction
    {
        private bool run;
        private int moveRate;
        private Loc destination;
        private Loc curPos;

        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        protected override int AnimFrameType { get { return GraphicsManager.WalkAction; } }

        public bool Complete { get { return destination == curPos; } }

        public WalkToPositionGroundAction(Loc loc, Dir8 dir, bool run, int moveRate, FrameTick prevTime, Loc destination)
        {
            MapLoc = loc;
            CharDir = dir;
            this.run = run;
            ActionTime = prevTime;
            this.moveRate = moveRate;
            this.destination = destination;
            curPos = MapLoc;
        }

        public override void UpdateTime(FrameTick elapsedTime)
        {
            base.UpdateTime(elapsedTime * (run ? 2 : 1));
        }

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
                NextAction = new IdleGroundAction(MapLoc, CharDir);
        }

        public override void Update(FrameTick elapsedTime)
        {
            Loc movediff = destination - curPos;

            //Get the difference between the current position and destination
            double x = movediff.X / Math.Sqrt(movediff.DistSquared());
            double y = movediff.Y / Math.Sqrt(movediff.DistSquared());

            //Ignore translation on a given axis if its already at the correct position on that axis.
            // Mainly because we're rounding the values to the nearest integer, and might overshoot otherwise.
            if (movediff.X == 0)
                x = 0;
            if (movediff.Y == 0)
                y = 0;

            //Convert the floating point number to the biggest nearby integer value, and keep its sign
            Loc movevec = new Loc();
            movevec.X = (int)(Math.Ceiling(Math.Abs(x)) * Math.Sign(x));
            movevec.Y = (int)(Math.Ceiling(Math.Abs(y)) * Math.Sign(y));


            Loc checkedmove = new Loc();
            //Constrain the move vector components to the components of the position difference vector to
            // ensure we don't overshoot because of the move rate
            checkedmove.X = Math.Min(Math.Abs(movediff.X), Math.Abs(movevec.X * moveRate)) * Math.Sign(movevec.X);
            checkedmove.Y = Math.Min(Math.Abs(movediff.Y), Math.Abs(movevec.Y * moveRate)) * Math.Sign(movevec.Y);

            //Update facing direction. Ignore none, since it crashes the game.
            Dir8 newdir = movevec.ApproximateDir8();
            if (newdir != Dir8.None)
                CharDir = newdir;

            Move = checkedmove;
            curPos += Move; //Increment our internal current position, since we have no ways of knowing where we are otherwise..
        }
    }


}
