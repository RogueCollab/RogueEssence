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
        public abstract int AnimFrameType { get; }
        public virtual int FrameMethod(List<CharAnimFrame> frames) { return zeroFrame(frames); }

        public Loc MapLoc { get { return collider.Start; } set { collider.Start = value; } }
        public Loc Move { get; protected set; }
        public int HeightMove { get; protected set; }

        protected Rect collider;
        public Rect Collider { get { return collider; } }
        public int LocHeight { get; set; }
        public Dir8 CharDir { get; set; }

        public FrameTick ActionTime { get; set; }
        public abstract bool Complete { get; }

        protected Dir8 dirOffset;
        protected int opacity;

        protected Loc drawOffset;
        public Loc DrawOffset { get { return drawOffset; } }


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
        public virtual void Begin(CharID appearance)
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

        public virtual void UpdateFrameInternal() { }
        public void UpdateFrame()
        {
            drawOffset = new Loc();
            dirOffset = Dir8.Down;
            opacity = 255;

            UpdateFrameInternal();
        }

        public void UpdateDrawEffects(HashSet<DrawEffect> drawEffects)
        {
            // Support the other draw effects later?

            if (drawEffects.Contains(DrawEffect.Absent))
                opacity = 0;
            else if (drawEffects.Contains(DrawEffect.Transparent))
                opacity = 128;

            if (drawEffects.Contains(DrawEffect.Shaking))
            {
                int sway = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(1) % 8);
                drawOffset.X += (sway > 4) ? (6 - sway) : (sway - 2);
            }
            if (drawEffects.Contains(DrawEffect.Trembling))
            {
                int sway = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(2) % 2);
                drawOffset.X += sway;
            }
            if (drawEffects.Contains(DrawEffect.Spinning))
                dirOffset = (Dir8)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(2) % 8);
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

        public Loc GetSheetOffset(CharSheet sheet)
        {
            //draw sprite at current frame
            return sheet.GetSheetOffset(AnimFrameType, true, DirExt.AddAngles(CharDir, dirOffset), FrameMethod);
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
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }

        public int Override;

        public override int AnimFrameType { get { return Override > -1 ? Override : GraphicsManager.GlobalIdle; } }
        public override bool Complete => true;

        public IdleGroundAction(Loc loc, Dir8 dir)
        {
            MapLoc = loc;
            CharDir = dir;
            Override = -1;
        }

        public IdleGroundAction(Loc loc, int height, Dir8 dir)
        {
            MapLoc = loc;
            LocHeight = height;
            CharDir = dir;
            Override = -1;
        }

        public override void UpdateInput(GameAction action)
        {
            //trigger walk if ordered to
            if (action.Type == GameAction.ActionType.Move)
                NextAction = new WalkGroundAction(MapLoc, LocHeight, action.Dir, action[0] != 0, action[1], new FrameTick());
        }

        public void RestartAnim()
        {
            ActionTime = FrameTick.Zero;
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
        private int speed;
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override int AnimFrameType { get { return GraphicsManager.WalkAction; } }
        public override bool Complete => true;
        public WalkGroundAction(Loc loc, Dir8 dir, bool run, int speed, FrameTick prevTime)
        {
            MapLoc = loc;
            CharDir = dir;
            this.run = run;
            this.speed = speed;
            ActionTime = prevTime;
        }
        public WalkGroundAction(Loc loc, int height, Dir8 dir, bool run, int speed, FrameTick prevTime)
        {
            MapLoc = loc;
            LocHeight = height;
            CharDir = dir;
            this.run = run;
            this.speed = speed;
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
                speed = action[1];
            }
            else if (action.Type == GameAction.ActionType.None)//start skid if ordered to
                NextAction = new IdleGroundAction(MapLoc, LocHeight, CharDir);// new SkidGroundAction(MapLoc, CharDir, ActionTime);
            // attempting to interact does not halt movement
            // but we don't want players to notice
            // so don't change direction or run state until the frame after
            // note that this only applies to interaction attempts that fail to find something to interact with.
        }

        public override void Update(FrameTick elapsedTime)
        {
            //set the character's projected movement
            Move = CharDir.GetLoc() * speed;
        }
    }

    [Serializable]
    public class IdleNoAnim : GroundAction
    {
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, 0, false); //Don't animate
        }
        public override int AnimFrameType { get { return GraphicsManager.GlobalIdle; } }
        public override bool Complete => true;
        public IdleNoAnim(Loc loc, Dir8 dir)
        {
            MapLoc = loc;
            CharDir = dir;
        }
        public IdleNoAnim(Loc loc, int height, Dir8 dir)
        {
            MapLoc = loc;
            LocHeight = height;
            CharDir = dir;
        }

        public override void UpdateInput(GameAction action)
        {
            //trigger walk if ordered to
            if (action.Type == GameAction.ActionType.Move)
                NextAction = new WalkGroundAction(MapLoc, LocHeight, action.Dir, action[0] != 0, action[1], new FrameTick());
        }
    }


    [Serializable]
    public class SkidGroundAction : GroundAction
    {
        private FrameTick skidTime;

        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override int AnimFrameType { get { return GraphicsManager.WalkAction; } }
        public override bool Complete => true;
        public SkidGroundAction(Loc loc, Dir8 dir, FrameTick prevTime)
        {
            MapLoc = loc;
            CharDir = dir;
            ActionTime = prevTime;
            skidTime = prevTime;
        }
        public SkidGroundAction(Loc loc, int height, Dir8 dir, FrameTick prevTime)
        {
            MapLoc = loc;
            LocHeight = height;
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
                NextAction = new WalkGroundAction(MapLoc, LocHeight,action.Dir, action[0] != 0, action[1], ActionTime);
            else
            {
                int prevTime = (skidTime / AnimTotalTime).ToFrames();
                int newTime = (ActionTime / AnimTotalTime).ToFrames();
                if (prevTime < newTime)//stop skid if timed out
                    NextAction = new IdleGroundAction(MapLoc, LocHeight, CharDir);
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
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, !Loop);
        }
        public override int AnimFrameType { get { return AnimID; } }

        public override bool Complete { get { return !Loop && ActionTime >= AnimTotalTime; } }

        int AnimID;
        bool Loop;

        public IdleAnimGroundAction(Loc pos, Dir8 dir, int animid, bool loop)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
            Loop = loop;
        }

        public IdleAnimGroundAction(Loc pos, int height, Dir8 dir, int animid, bool loop)
        {
            MapLoc = pos;
            LocHeight = height;
            CharDir = dir;
            AnimID = animid;
            Loop = loop;
        }

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
                NextAction = new IdleGroundAction(MapLoc, LocHeight, CharDir);
        }
    }

    [Serializable]
    public class PoseGroundAction : GroundAction
    {
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, Math.Min(ActionTime.Ticks, FrameTick.FrameToTick(AnimReturnTime)), true);
        }
        public override int AnimFrameType { get { return AnimID; } }
        public override bool Complete { get { return ActionTime >= AnimTotalTime; } }

        int AnimID;

        public PoseGroundAction(Loc pos, Dir8 dir, int animid)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
        }

        public PoseGroundAction(Loc pos, int height, Dir8 dir, int animid)
        {
            MapLoc = pos;
            LocHeight = height;
            CharDir = dir;
            AnimID = animid;
        }

        public override void UpdateInput(GameAction action)
        {

        }
    }

    [Serializable]
    public class FrameGroundAction : GroundAction
    {
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return Math.Clamp(Frame, 0, frames.Count - 1);
        }
        public override int AnimFrameType { get { return AnimID; } }
        public override bool Complete { get { return true; } }

        int AnimID;
        int Frame;

        public FrameGroundAction(Loc pos, Dir8 dir, int animid, int frame)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
            Frame = frame;
        }

        public FrameGroundAction(Loc pos, int height, Dir8 dir, int animid, int frame)
        {
            MapLoc = pos;
            LocHeight = height;
            CharDir = dir;
            AnimID = animid;
            Frame = frame;
        }

        public override void UpdateInput(GameAction action)
        {

        }
    }

    [Serializable]
    public class ReverseGroundAction : GroundAction
    {
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            long totalTick = FrameTick.FrameToTick(AnimTotalTime);
            return CharSheet.TrueFrame(frames, totalTick - (ActionTime.Ticks % totalTick), true);
        }
        public override int AnimFrameType { get { return AnimID; } }
        public override bool Complete { get { return ActionTime >= AnimTotalTime; } }

        int AnimID;

        public ReverseGroundAction(Loc pos, Dir8 dir, int animid)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
        }

        public ReverseGroundAction(Loc pos, int height, Dir8 dir, int animid)
        {
            MapLoc = pos;
            LocHeight = height;
            CharDir = dir;
            AnimID = animid;
        }

        public override void UpdateInput(GameAction action)
        {

        }
    }


    [Serializable]
    public class AnimateToPositionGroundAction : GroundAction
    {
        private float animSpeed;
        private float moveRate;
        private Loc goalDiff;
        private Loc curDiff;
        private int goalHeightDiff;
        private int curHeightDiff;
        private int framesPassed;
        private GroundAction baseAction;

        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return baseAction.FrameMethod(frames);
        }
        public override int AnimFrameType { get { return baseAction.AnimFrameType; } }

        public override bool Complete { get { return (curDiff == goalDiff) && (curHeightDiff == goalHeightDiff); } }

        public AnimateToPositionGroundAction(GroundAction animType, float animSpeed, float moveRate, FrameTick prevTime, Loc destination, int destHeight)
        {
            this.baseAction = animType;
            MapLoc = animType.MapLoc;
            CharDir = animType.CharDir;
            LocHeight = animType.LocHeight;
            this.animSpeed = animSpeed;
            ActionTime = prevTime;
            baseAction.ActionTime = prevTime;
            this.moveRate = moveRate;
            this.goalDiff = destination - MapLoc;
            this.goalHeightDiff = destHeight - LocHeight;
        }

        public override void Begin(CharID appearance)
        {
            base.Begin(appearance);
            baseAction.Begin(appearance);
        }

        public override void UpdateFrameInternal()
        {
            baseAction.UpdateFrameInternal();
        }

        public override void UpdateTime(FrameTick elapsedTime)
        {
            base.UpdateTime(new FrameTick((long)(elapsedTime.Ticks * animSpeed)));
            baseAction.UpdateTime(new FrameTick((long)(elapsedTime.Ticks * animSpeed)));
        }

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
                NextAction = new IdleGroundAction(MapLoc, LocHeight, CharDir);
        }

        public override void Update(FrameTick elapsedTime)
        {
            framesPassed++;
            if (goalDiff == Loc.Zero && goalHeightDiff == 0)
                return;

            int highestScalar = Math.Abs(goalHeightDiff);
            if (highestScalar < Math.Abs(goalDiff.X))
                highestScalar = Math.Abs(goalDiff.X);
            if (highestScalar < Math.Abs(goalDiff.Y))
                highestScalar = Math.Abs(goalDiff.Y);

            float mainMove = moveRate * framesPassed;
            int moveX = (int)Math.Round(mainMove * goalDiff.X / highestScalar);
            int moveY = (int)Math.Round(mainMove * goalDiff.Y / highestScalar);
            Loc newDiff = new Loc(moveX, moveY);
            if (mainMove >= highestScalar)
                newDiff = goalDiff;
            Move = newDiff - curDiff;
            curDiff = newDiff;

            int newHeightDiff = (int)Math.Round(mainMove * goalHeightDiff / highestScalar);
            if (mainMove >= highestScalar)
                newHeightDiff = goalHeightDiff;
            HeightMove = newHeightDiff - curHeightDiff;
            curHeightDiff = newHeightDiff;
            baseAction.Update(elapsedTime);
        }
    }


    [Serializable]
    public class HopGroundAction : GroundAction
    {
        public override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, 0, true);
        }
        public override int AnimFrameType { get { return AnimID; } }
        public override bool Complete { get { return ActionTime >= Duration; } }

        int AnimID;
        int Height;
        int StartHeight;
        int Duration;

        public HopGroundAction(Loc pos, Dir8 dir, int animid, int topHeight, int duration)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
            Height = topHeight;
            Duration = duration;
        }

        public HopGroundAction(Loc pos, int startHeight, Dir8 dir, int animid, int topHeight, int duration)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
            StartHeight = startHeight;
            Height = topHeight;
            Duration = duration;
        }

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
                NextAction = new IdleGroundAction(MapLoc, StartHeight, CharDir);
        }

        public override void Update(FrameTick elapsedTime)
        {
            if (ActionTime < Duration / 2)
                LocHeight = MathUtils.DivUp(Height * ActionTime.ToFrames() * 2, Duration) + StartHeight;
            else
                LocHeight = MathUtils.DivUp(Height * (Duration - ActionTime.ToFrames()) * 2, Duration) + StartHeight;
        }
    }
}
