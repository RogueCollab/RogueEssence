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
        protected virtual int FrameMethod(List<CharAnimFrame> frames) { return zeroFrame(frames); }

        public Loc MapLoc { get { return collider.Start; } set { collider.Start = value; } }
        public Loc Move { get; protected set; }

        protected Rect collider;
        public Rect Collider { get { return collider; } }
        public int LocHeight { get; set; }
        public Dir8 CharDir { get; set; }

        public FrameTick ActionTime { get; protected set; }
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
        public void Begin(CharID appearance)
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

        public int Override;

        public override int AnimFrameType { get { return Override > -1 ? Override : GraphicsManager.GlobalIdle; } }
        public override bool Complete => true;

        public IdleGroundAction(Loc loc, Dir8 dir)
        {
            MapLoc = loc;
            CharDir = dir;
            Override = -1;
        }

        public override void UpdateInput(GameAction action)
        {
            //trigger walk if ordered to
            if (action.Type == GameAction.ActionType.Move)
                NextAction = new WalkGroundAction(MapLoc, action.Dir, action[0] != 0, action[1], new FrameTick());
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
        protected override int FrameMethod(List<CharAnimFrame> frames)
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
                NextAction = new IdleGroundAction(MapLoc, CharDir);// new SkidGroundAction(MapLoc, CharDir, ActionTime);
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
        protected override int FrameMethod(List<CharAnimFrame> frames)
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

        public override void UpdateInput(GameAction action)
        {
            //trigger walk if ordered to
            if (action.Type == GameAction.ActionType.Move)
                NextAction = new WalkGroundAction(MapLoc, action.Dir, action[0] != 0, action[1], new FrameTick());
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
        public override int AnimFrameType { get { return GraphicsManager.WalkAction; } }
        public override bool Complete => true;
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
                NextAction = new WalkGroundAction(MapLoc, action.Dir, action[0] != 0, action[1], ActionTime);
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

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
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
        public override int AnimFrameType { get { return AnimID; } }
        public override bool Complete { get { return ActionTime >= AnimTotalTime; } }

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
    public class AnimateToPositionGroundAction : GroundAction
    {
        private int animType;
        private float animSpeed;
        private int moveRate;
        private Loc goalDiff;
        private Loc curDiff;
        private int framesPassed;

        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override int AnimFrameType { get { return animType; } }

        public override bool Complete { get { return curDiff == goalDiff; } }

        public AnimateToPositionGroundAction(int animType, Loc loc, Dir8 dir, float animSpeed, int moveRate, FrameTick prevTime, Loc destination)
        {
            this.animType = animType;
            MapLoc = loc;
            CharDir = dir;
            this.animSpeed = animSpeed;
            ActionTime = prevTime;
            this.moveRate = moveRate;
            this.goalDiff = destination - MapLoc;
        }

        public override void UpdateTime(FrameTick elapsedTime)
        {
            base.UpdateTime(new FrameTick((long)(elapsedTime.Ticks * animSpeed)));
        }

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
                NextAction = new IdleGroundAction(MapLoc, CharDir);
        }

        public override void Update(FrameTick elapsedTime)
        {
            framesPassed++;
            if (goalDiff == Loc.Zero)
                return;

            bool vertical = Math.Abs(goalDiff.Y) > Math.Abs(goalDiff.X);
            int mainMove = moveRate * framesPassed;
            int subMove = (int)Math.Abs(Math.Round((double)moveRate * framesPassed * goalDiff.GetScalar(vertical ? Axis4.Horiz : Axis4.Vert) / goalDiff.GetScalar(vertical ? Axis4.Vert : Axis4.Horiz)));
            Loc newDiff = new Loc((vertical ? subMove : mainMove) * Math.Sign(goalDiff.X), (vertical ? mainMove : subMove) * Math.Sign(goalDiff.Y));
            if (mainMove >= Math.Abs(goalDiff.GetScalar(vertical ? Axis4.Vert : Axis4.Horiz)))
                newDiff = goalDiff;
            Move = newDiff - curDiff;
            curDiff = newDiff;
        }
    }


    [Serializable]
    public class HopGroundAction : GroundAction
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, 0, true);
        }
        public override int AnimFrameType { get { return AnimID; } }
        public override bool Complete { get { return ActionTime >= Duration; } }

        int AnimID;
        int Height;
        int Duration;

        public HopGroundAction(Loc pos, Dir8 dir, int animid, int height, int duration)
        {
            MapLoc = pos;
            CharDir = dir;
            AnimID = animid;
            Height = height;
            Duration = duration;
        }

        public override void UpdateInput(GameAction action)
        {
            if (Complete)
                NextAction = new IdleGroundAction(MapLoc, CharDir);
        }

        public override void Update(FrameTick elapsedTime)
        {
            if (ActionTime < Duration / 2)
                LocHeight = MathUtils.DivUp(Height * ActionTime.ToFrames() * 2, Duration);
            else
                LocHeight = MathUtils.DivUp(Height * (Duration - ActionTime.ToFrames()) * 2, Duration);
        }
    }
}
