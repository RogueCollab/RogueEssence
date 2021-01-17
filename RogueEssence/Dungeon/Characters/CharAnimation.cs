using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Dungeon
{
    public struct VisionLoc
    {
        public Loc Loc;
        public float Weight;

        public VisionLoc(Loc loc, float weight)
        {
            Loc = loc;
            Weight = weight;
        }
    }


    [Serializable]
    public abstract class CharAnimation
    {

        protected abstract int AnimFrameType { get; }
        protected virtual int FrameMethod(List<CharAnimFrame> frames) { return zeroFrame(frames); }

        public bool MajorAnim { get; set; }
        public abstract Loc CharLoc { get; set; }
        public abstract Loc CharLocFrom { get; }
        public Dir8 CharDir { get; set; }

        public abstract bool ActionPassed { get; }
        public abstract bool ActionDone { get; }

        public Action OnFinish;

        public virtual bool WantsToEnd() { return ActionDone; }

        protected FrameTick PrevActionTime;
        protected FrameTick ActionTime;

        private int charFrameType;
        private CharSheet.DetermineFrame determineFrame;
        protected Dir8 dirOffset;
        protected int opacity;

        protected Loc drawOffset;
        public Loc MapLoc { get; protected set; }
        public int LocHeight { get; protected set; }
        public Loc DrawOffset { get { return drawOffset; } }

        public int AnimRushTime { get; private set; }
        public int AnimHitTime { get; private set; }
        public int AnimReturnTime { get; private set; }
        public int AnimTotalTime { get; private set; }

        protected virtual bool TakesPrevActionTime() { return false; }
        protected virtual FrameTick AddPrevActionTime() { return (ActionTime + PrevActionTime); }
        public void PickUpFrom(MonsterID appearance, CharAnimation prevAnim)
        {
            AnimRushTime = GraphicsManager.GetChara(appearance).GetRushTime(AnimFrameType, CharDir);
            AnimHitTime = GraphicsManager.GetChara(appearance).GetHitTime(AnimFrameType, CharDir);
            AnimReturnTime = GraphicsManager.GetChara(appearance).GetReturnTime(AnimFrameType, CharDir);
            AnimTotalTime = GraphicsManager.GetChara(appearance).GetTotalTime(AnimFrameType, CharDir);
            if (prevAnim.TakesPrevActionTime() && TakesPrevActionTime())
                PrevActionTime = prevAnim.AddPrevActionTime();
        }
        public virtual bool ProcessInterruptingAction(CharAnimation newAnim)
        {
            return newAnim.MajorAnim;
        }

        protected virtual bool TakesMovementSpeed() { return false; }
        protected virtual void UpdateInternal() { }
        public void Update(FrameTick elapsedTime, MonsterID appearance, int movementSpeed)
        {
            if (TakesMovementSpeed())
            {
                if (movementSpeed < 0)
                    elapsedTime /= 2;
                else if (movementSpeed > 0)
                    elapsedTime *= 2;
            }

            ActionTime += elapsedTime;

            UpdateInternal();
        }

        protected abstract void UpdateFrameInternal();
        public void UpdateFrame()
        {
            charFrameType = AnimFrameType;
            determineFrame = FrameMethod;
            drawOffset = new Loc();
            dirOffset = Dir8.Down;
            opacity = 255;
            LocHeight = 0;

            UpdateFrameInternal();
        }

        protected virtual bool AllowFrameTypeDrawEffects() { return false; }
        public void UpdateDrawEffects(HashSet<DrawEffect> drawEffects)
        {
            if (AllowFrameTypeDrawEffects())
            {
                if (drawEffects.Contains(DrawEffect.Hurt))
                {
                    charFrameType = GraphicsManager.HurtAction;
                    determineFrame = totalFrameTickFrame;
                }
                else if (drawEffects.Contains(DrawEffect.Sleeping))
                {
                    charFrameType = GraphicsManager.SleepAction;
                    determineFrame = totalFrameTickFrame;
                }
                else if (drawEffects.Contains(DrawEffect.Charging))
                {
                    charFrameType = GraphicsManager.ChargeAction;
                    determineFrame = totalFrameTickFrame;
                }
            }

            if (drawEffects.Contains(DrawEffect.Absent))
                opacity = 0;

            if (drawEffects.Contains(DrawEffect.Shaking))
            {
                int sway = (int)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(1) % 8);
                drawOffset.X += (sway > 4) ? (6 - sway) : (sway - 2);
            }
            if (drawEffects.Contains(DrawEffect.Spinning))
                dirOffset = (Dir8)(GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(2) % 8);

            if (drawEffects.Contains(DrawEffect.Stopped))
                determineFrame = zeroFrame;
        }



        public void Draw(SpriteBatch spriteBatch, Loc offset, CharSheet sheet)
        {
            Loc drawLoc = GetDrawLoc(sheet, offset);
            drawLoc.Y -= LocHeight;
            
            //draw sprite at current frame
            sheet.DrawChar(spriteBatch, charFrameType, DirExt.AddAngles(CharDir, dirOffset), drawLoc.ToVector2(), determineFrame, Microsoft.Xna.Framework.Color.White * ((float)opacity / 255));
        }
        public Loc GetActionPoint(CharSheet sheet, ActionPointType pointType)
        {
            Loc midTileOffset = new Loc(GraphicsManager.TileSize / 2);
            return MapLoc + midTileOffset + drawOffset + sheet.GetActionPoint(charFrameType, DirExt.AddAngles(CharDir, dirOffset), pointType, FrameMethod);
        }

        private int totalFrameTickFrame(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, (long)GraphicsManager.TotalFrameTick, false);
        }
        private int zeroFrame(List<CharAnimFrame> frames)
        {
            return 0;
        }

        public Loc GetDrawLoc(CharSheet sheet, Loc offset)
        {
            return new Loc(MapLoc.X + GraphicsManager.TileSize / 2 - sheet.TileWidth / 2,
                MapLoc.Y + GraphicsManager.TileSize / 2 - sheet.TileHeight / 2) + drawOffset - offset;
        }

        public void GetCurrentSprite(CharSheet sheet, out int currentAnim, out int currentTime, out int currentFrame)
        {
            currentAnim = charFrameType;
            currentTime = ActionTime.ToFrames();
            currentFrame = sheet.GetCurrentFrame(charFrameType, DirExt.AddAngles(CharDir, dirOffset), FrameMethod);
        }

        //location that this character is in, for visibility purposes
        //if any of these locations are seen by the player, the character is considered seen by the player
        public virtual IEnumerable<Loc> GetLocsVisible()
        {
            yield return CharLoc;
        }

        //location that this character is in, for vision purposes
        //any locations seen by these locs are seen by the player
        public virtual IEnumerable<VisionLoc> GetVisionLocs()
        {
            yield return new VisionLoc(CharLoc, 1f);
        }

    }

    public abstract class StaticCharAnimation : CharAnimation
    {
        public Loc AnimLoc { get; set; }
        public override Loc CharLocFrom { get { return AnimLoc; } }
        public override Loc CharLoc { get { return AnimLoc; } set { AnimLoc = value; } }

        public bool FallShort { get; set; }

    }


    public abstract class MovingCharAnimation : CharAnimation
    {
        public Loc FromLoc { get; set; }
        public Loc ToLoc { get; set; }
        public override Loc CharLocFrom { get { return FromLoc; } }
        public override Loc CharLoc { get { return ToLoc; } set { ToLoc = value; } }

        public override IEnumerable<Loc> GetLocsVisible()
        {
            yield return FromLoc;
            yield return ToLoc;
        }
    }

    public abstract class RecoilingAnimation : MovingCharAnimation
    {
        public Loc RecoilLoc { get; set; }
        public override Loc CharLoc { get { return RecoilLoc; } set { RecoilLoc = value; } }

        public override IEnumerable<Loc> GetLocsVisible()
        {
            yield return FromLoc;
            yield return ToLoc;
            yield return RecoilLoc;
        }
    }

    public class CharAnimIdle : StaticCharAnimation
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override bool ActionPassed { get { return true; } }
        public override bool ActionDone { get { return true; } }

        protected override bool TakesMovementSpeed() { return true; }

        public override bool WantsToEnd() { return false; }

        public CharAnimIdle(Loc loc, Dir8 dir) { AnimLoc = loc; CharDir = dir; }

        protected override int AnimFrameType { get { return GraphicsManager.GlobalIdle; } }
        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
        }
        protected override bool AllowFrameTypeDrawEffects() { return true; }
    }

    public class CharAnimPose : StaticCharAnimation
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, Math.Min(ActionTime.Ticks, FrameTick.FrameToTick(AnimReturnTime)), true);
        }
        public override bool ActionPassed { get { return ActionTime >= PoseTime; } }
        public override bool ActionDone { get { return ActionTime >= PoseTime; } }

        public CharAnimPose() { }
        public CharAnimPose(Loc loc, Dir8 dir, int frameType, int poseTime) { AnimLoc = loc; CharDir = dir; BaseFrameType = frameType; PoseTime = poseTime; }

        public int PoseTime { get; set; }
        public int BaseFrameType { get; set; }
        protected override int AnimFrameType { get { return BaseFrameType; } }
        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
        }
    }

    public class CharAnimAction : StaticCharAnimation
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, true);
        }
        public override bool ActionPassed { get { return ActionTime >= AnimHitTime; } }
        public override bool ActionDone { get { return ActionTime >= AnimTotalTime; } }

        public CharAnimAction() { }
        public CharAnimAction(Loc loc, Dir8 dir, int frameType) { AnimLoc = loc; CharDir = dir; BaseFrameType = frameType; }

        public int BaseFrameType { get; set; }
        protected override int AnimFrameType { get { return BaseFrameType; } }
        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
        }
    }


    public class CharLungeAction : StaticCharAnimation
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, true);
        }
        public override bool ActionPassed { get { return ActionTime >= AnimHitTime; } }
        public override bool ActionDone { get { return ActionTime >= AnimTotalTime; } }

        public int BaseFrameType { get; set; }
        protected override int AnimFrameType { get { return BaseFrameType; } }
        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;

            int farthest_distance = GraphicsManager.TileSize * (FallShort ? 1 : 2) / 3;
            Loc toOffset = CharDir.GetLoc() * farthest_distance;

            if (ActionTime < AnimRushTime)
            {
                //dont do anything; the animation itself will take care of pull-back
            }
            else if (ActionTime < AnimHitTime)
            {
                double intb = (double)(ActionTime - AnimRushTime).FractionOf(AnimHitTime - AnimRushTime);
                Loc newLoc = new Loc(AnimMath.Lerp(0, toOffset.X, intb), AnimMath.Lerp(0, toOffset.Y, intb));
                drawOffset = newLoc;
            }
            else if (ActionTime < AnimReturnTime)
                drawOffset = toOffset;
            else
            {
                double intb = (double)(ActionTime - AnimReturnTime).FractionOf(AnimTotalTime - AnimReturnTime);
                Loc newLoc = new Loc(AnimMath.Lerp(toOffset.X, 0, intb), AnimMath.Lerp(toOffset.Y, 0, intb));
                drawOffset = newLoc;
            }
        }
    }

    public class CharAnimHurt : StaticCharAnimation
    {
        public const int ANIM_TIME = 30;
        public const int PASS_TIME = 0;
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override bool ActionPassed { get { return ActionTime >= PASS_TIME; } }
        public override bool ActionDone { get { return ActionTime >= ANIM_TIME; } }

        protected override int AnimFrameType { get { return GraphicsManager.HurtAction; } }
        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
            if (ActionTime.FractionOf(2, ANIM_TIME) % 2 == 0)
                drawOffset = drawOffset + CharDir.Reverse().GetLoc();
        }
    }

    public class CharAnimDefeated : StaticCharAnimation
    {
        public override bool ActionPassed { get { return ActionDone; } }
        public override bool ActionDone { get { return ActionTime >= AnimTime; } }

        public int AnimTime;

        protected override int AnimFrameType { get { return GraphicsManager.HurtAction; } }
        protected override void UpdateFrameInternal()
        {
            if (ActionTime.DivOf(10) % 2 == 0)
                drawOffset = drawOffset + CharDir.Reverse().GetLoc();
            if (ActionTime.FractionOf(4, AnimTime) > 0)
                opacity = 128;
            MapLoc = AnimLoc * GraphicsManager.TileSize;
        }
    }

    public class CharAnimDrop : StaticCharAnimation
    {
        const int MAX_TILE_HEIGHT = 8;
        public const int ANIM_TIME = 12;
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override bool ActionPassed { get { return ActionDone; } }
        public override bool ActionDone { get { return ActionTime >= ANIM_TIME; } }

        protected override int AnimFrameType { get { return 0; } }
        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
            LocHeight = MAX_TILE_HEIGHT * GraphicsManager.TileSize-(int)ActionTime.FractionOf(MAX_TILE_HEIGHT * GraphicsManager.TileSize, ANIM_TIME);
        }
    }

    public class CharAnimFly : StaticCharAnimation
    {
        const int MAX_TILE_HEIGHT = 8;
        public const int ANIM_TIME = 24;
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override bool ActionPassed { get { return ActionTime >= ANIM_TIME - 1; } }
        public override bool ActionDone { get { return ActionTime >= ANIM_TIME; } }
        protected override int AnimFrameType { get { return GraphicsManager.ChargeAction; } }

        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
            LocHeight = (int)ActionTime.FractionOf(MAX_TILE_HEIGHT * GraphicsManager.TileSize, ANIM_TIME);
        }
    }

    public class CharAnimSpin : StaticCharAnimation
    {
        public const int SPINS = 3;
        public const int ANIM_TIME = 24;
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, ActionTime.Ticks, false);
        }
        public override bool ActionPassed { get { return ActionDone; } }
        public override bool ActionDone { get { return ActionTime >= ANIM_TIME; } }
        protected override int AnimFrameType { get { return GraphicsManager.ChargeAction; } }

        protected override void UpdateFrameInternal()
        {
            MapLoc = AnimLoc * GraphicsManager.TileSize;
            dirOffset = (Dir8)(ActionTime.FractionOf(8 * SPINS, ANIM_TIME) % 8);
        }
    }

    public class CharAnimWalk : MovingCharAnimation
    {
        const int FINISH_TIME = 12;
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.TrueFrame(frames, PrevActionTime.Ticks + ActionTime.Ticks * ((SpeedMult > 1) ? 2 : 1), false);
        }
        public override bool ActionPassed { get { return true; } }
        public override bool ActionDone { get { return ActionTime >= ((SpeedMult != 0) ? (FINISH_TIME * MidLocs.Count / SpeedMult) : 0); } }

        public override bool WantsToEnd()
        {
            return ActionTime >= FINISH_TIME * MidLocs.Count && ActionTime >= AnimTotalTime;
        }

        public int SpeedMult;
        public List<Loc> MidLocs;
        public List<Dir8> MidDirs;

        protected override bool TakesPrevActionTime() { return true; }
        protected override FrameTick AddPrevActionTime() { return (ActionTime * SpeedMult + PrevActionTime); }

        protected override int AnimFrameType { get { return GraphicsManager.WalkAction; } }

        public CharAnimWalk(Loc fromLoc, Loc toLoc, Dir8 dir, int speedMult)
        {
            MidLocs = new List<Loc>();
            MidDirs = new List<Dir8>();
            FromLoc = fromLoc;
            MidLocs.Add(fromLoc);
            ToLoc = toLoc;
            CharDir = dir;
            MidDirs.Add(dir);
            SpeedMult = speedMult;
            MajorAnim = true;
        }

        protected override void UpdateFrameInternal()
        {
            FrameTick trueTime = ActionTime * SpeedMult;
            int curSegment = Math.Min(trueTime.ToFrames() / FINISH_TIME, MidLocs.Count-1);
            Dir8 midDir = MidDirs[curSegment];
            Loc fromLoc = MidLocs[curSegment];
            Loc toLoc = curSegment+1 < MidLocs.Count ? MidLocs[curSegment + 1] : ToLoc;
            dirOffset = (Dir8)(((int)midDir - (int)CharDir + DirExt.DIR8_COUNT) % DirExt.DIR8_COUNT);
            FrameTick segmentTime = trueTime - FINISH_TIME * curSegment;
            double intb = Math.Min(1.0, (double)segmentTime.FractionOf(FINISH_TIME));
            MapLoc = new Loc(AnimMath.Lerp(fromLoc.X * GraphicsManager.TileSize, toLoc.X * GraphicsManager.TileSize, intb),
                AnimMath.Lerp(fromLoc.Y * GraphicsManager.TileSize, toLoc.Y * GraphicsManager.TileSize, intb));
        }
        public override IEnumerable<VisionLoc> GetVisionLocs()
        {
            float diff = ActionTime.FractionOf(FINISH_TIME);
            if (diff > 0f)
                yield return new VisionLoc(ToLoc, diff);
            if (diff < 1f)
                yield return new VisionLoc(FromLoc, 1f - diff); 
        }

        public override IEnumerable<Loc> GetLocsVisible()
        {
            float diff = ActionTime.FractionOf(FINISH_TIME);
            if (diff > 0f)
                yield return ToLoc;
            if (diff < 1f)
                yield return FromLoc;
        }

        public override bool ProcessInterruptingAction(CharAnimation newAnim)
        {
            CharAnimWalk walkAnim = newAnim as CharAnimWalk;
            if (walkAnim == null)
                return newAnim.MajorAnim;

            MidLocs.Add(ToLoc);
            MidDirs.Add(CharDir);

            ToLoc = walkAnim.ToLoc;
            CharDir = walkAnim.CharDir;

            SpeedMult = MidLocs.Count;

            return false;
        }
    }
    public class CharAnimWarp : MovingCharAnimation
    {
        const int ANIM_TIME = 60;
        const int SPINS = 3;
        const int MAX_TILE_HEIGHT = 8;
        public override bool ActionPassed { get { return ActionDone; } }
        public override bool ActionDone { get { return ActionTime >= ANIM_TIME; } }

        protected override int AnimFrameType { get { return GraphicsManager.IdleAction; } }
        protected override void UpdateFrameInternal()
        {
            dirOffset = (Dir8)(ActionTime.FractionOf(8 * SPINS, ANIM_TIME) % 8);
            if (ActionTime < ANIM_TIME / 2)
            {
                LocHeight = (int)(ActionTime.FractionOf(GraphicsManager.TileSize * MAX_TILE_HEIGHT * 2, ANIM_TIME));
                MapLoc = FromLoc * GraphicsManager.TileSize;
            }
            else
            {
                LocHeight = -(int)(ActionTime - ANIM_TIME).FractionOf(GraphicsManager.TileSize * MAX_TILE_HEIGHT * 2, ANIM_TIME);
                MapLoc = ToLoc * GraphicsManager.TileSize;
            }
        }
        public override IEnumerable<Loc> GetLocsVisible()
        {
            if (ActionTime < ANIM_TIME / 2)
                yield return FromLoc;
            else
                yield return ToLoc;
        }
        public override IEnumerable<VisionLoc> GetVisionLocs()
        {
            if (ActionTime < ANIM_TIME / 2)
                yield return new VisionLoc(FromLoc, 1f);
            else
                yield return new VisionLoc(ToLoc, 1f);
        }
    }

    public class CharAnimJump : MovingCharAnimation
    {
        const int ANIM_TIME = 30;
        public override bool ActionPassed { get { return ActionDone; } }
        public override bool ActionDone { get { return ActionTime >= ANIM_TIME; } }

        protected override int AnimFrameType { get { return GraphicsManager.IdleAction; } }
        protected override void UpdateFrameInternal()
        {
            if (FromLoc != ToLoc)
            {
                double maxDistance = Math.Sqrt(((FromLoc - ToLoc) * GraphicsManager.TileSize).DistSquared());
                LocHeight = AnimMath.GetArc(maxDistance / 2, ANIM_TIME, ActionTime.ToFrames());

                Loc diff = (ToLoc - FromLoc) * GraphicsManager.TileSize;
                diff = new Loc((int)(diff.X * ActionTime.FractionOf(ANIM_TIME)), (int)(diff.Y * ActionTime.FractionOf(ANIM_TIME)));
                MapLoc = diff + FromLoc * GraphicsManager.TileSize;
            }
            else
                MapLoc = FromLoc * GraphicsManager.TileSize;
        }
        public override IEnumerable<VisionLoc> GetVisionLocs()
        {
            float diff = ActionTime.FractionOf(ANIM_TIME);
            if (diff > 0f)
                yield return new VisionLoc(ToLoc, diff);
            if (diff < 1f)
                yield return new VisionLoc(FromLoc, 1f - diff);
        }
    }

    public class CharAnimSwitch : MovingCharAnimation
    {
        const int ANIM_TIME = 0;
        public override bool ActionPassed { get { return true; } }
        public override bool ActionDone { get { return true; } }

        protected override int AnimFrameType { get { return GraphicsManager.IdleAction; } }
        protected override void UpdateFrameInternal()
        {
            LocHeight = 0;
            MapLoc = ToLoc * GraphicsManager.TileSize;
        }
    }



    public abstract class DashAnimation : RecoilingAnimation
    {
        protected override int FrameMethod(List<CharAnimFrame> frames)
        {
            return CharSheet.FractionFrame(frames, ActionTime.Ticks, FrameTick.FrameToTick(AnimTotalTime), true);
        }
        public override bool ActionPassed { get { return ActionTime >= AnimRushTime; } }
        public override bool ActionDone { get { return ActionTime >= AnimTotalTime; } }

        public bool Overshoot { get; set; }
        public Action OnFinishLunge;
        private bool finishedLunge;
        
        protected override void UpdateInternal()
        {
            if (!finishedLunge && ActionTime >= AnimHitTime)
            {
                if (OnFinishLunge != null)
                    OnFinishLunge();
                finishedLunge = true;
            }
        }

        public int BaseFrameType { get; set; }
        protected override int AnimFrameType { get { return BaseFrameType; } }
        protected override void UpdateFrameInternal()
        {
            if (ActionTime < AnimRushTime)
            {
                //dont do anything; the animation itself will take care of pull-back
                MapLoc = FromLoc * GraphicsManager.TileSize;
            }
            else if (ActionTime < AnimHitTime)
            {
                double intb = (double)(ActionTime - AnimRushTime).FractionOf(AnimHitTime - AnimRushTime);
                Loc newLoc = new Loc(AnimMath.Lerp(FromLoc.X * GraphicsManager.TileSize, ToLoc.X * GraphicsManager.TileSize, intb),
                    AnimMath.Lerp(FromLoc.Y * GraphicsManager.TileSize, ToLoc.Y * GraphicsManager.TileSize, intb));
                MapLoc = newLoc;
            }
            else if (ActionTime < AnimReturnTime)
            {
                Loc newLoc = ToLoc * GraphicsManager.TileSize;
                MapLoc = newLoc;
            }
            else if (ActionTime < AnimTotalTime)
            {
                double intb = (double)(ActionTime - AnimReturnTime).FractionOf(AnimTotalTime - AnimReturnTime);
                Loc newLoc = new Loc(AnimMath.Lerp(ToLoc.X * GraphicsManager.TileSize, RecoilLoc.X * GraphicsManager.TileSize, intb),
                    AnimMath.Lerp(ToLoc.Y * GraphicsManager.TileSize, RecoilLoc.Y * GraphicsManager.TileSize, intb));
                MapLoc = newLoc;
            }
            else
            {
                MapLoc = RecoilLoc * GraphicsManager.TileSize;
            }
        }

        public override IEnumerable<VisionLoc> GetVisionLocs()
        {
            if (ActionTime < AnimRushTime)
                yield return new VisionLoc(FromLoc, 1f);
            else if (ActionTime < AnimHitTime)
            {
                float diff = (ActionTime - AnimRushTime).FractionOf(AnimHitTime - AnimRushTime);
                if (diff > 0f)
                    yield return new VisionLoc(RecoilLoc, diff);
                if (diff < 1f)
                    yield return new VisionLoc(FromLoc, 1f - diff);
            }
            else if (ActionTime < AnimReturnTime)
            {
                yield return new VisionLoc(RecoilLoc, 1f);//or maybe the ToLoc
            }
            else
            {
                yield return new VisionLoc(RecoilLoc, 1f);//or maybe the diff from ToLoc to RecoilLoc
            }
        }
    }

    public class CharAnimRush : DashAnimation
    {

    }

    public class CharAnimDropDash : DashAnimation
    {
        protected override void UpdateFrameInternal()
        {
            base.UpdateFrameInternal();

            if (ActionTime < AnimRushTime)
            {
                opacity = 0;
            }
            else if (ActionTime < AnimHitTime)
            {
                double intb = (double)(ActionTime - AnimRushTime).FractionOf(AnimHitTime - AnimRushTime);
                LocHeight = (int)(GraphicsManager.ScreenHeight * 0.6f * (1 - intb));
            }
        }
    }

    public class CharAnimGhostDash : DashAnimation
    {
        protected override void UpdateFrameInternal()
        {
            base.UpdateFrameInternal();

            if (ActionTime < AnimRushTime)
            {

            }
            else if (ActionTime < AnimHitTime)
            {
                opacity = 0;
            }
        }
    }

    public class CharAnimKnockback : RecoilingAnimation
    {
        const int KNOCKBACK_TIME_PER_TILE = 4;
        const int BOUNCE_TIME = 10;
        public override bool ActionPassed { get { return ActionDone; } }
        private int getActionTime()
        {
            int dist = (FromLoc - ToLoc).Dist8();
            int totalTime = KNOCKBACK_TIME_PER_TILE * dist;
            if (RecoilLoc != ToLoc)
                totalTime += BOUNCE_TIME;
            return totalTime;
        }
        public override bool ActionDone { get { return ActionTime >= getActionTime(); } }

        protected override int AnimFrameType { get { return GraphicsManager.HurtAction; } }
        protected override void UpdateFrameInternal()
        {
            int dist = (FromLoc - ToLoc).Dist8();
            int totalTime = KNOCKBACK_TIME_PER_TILE * dist;
            if (ActionTime < totalTime)
            {
                double intb = (double)ActionTime.FractionOf(totalTime);
                MapLoc = new Loc(AnimMath.Lerp(FromLoc.X * GraphicsManager.TileSize, ToLoc.X * GraphicsManager.TileSize, intb),
                    AnimMath.Lerp(FromLoc.Y * GraphicsManager.TileSize, ToLoc.Y * GraphicsManager.TileSize, intb));
                LocHeight = 0;
            }
            else
            {
                double intb = (double)(ActionTime - totalTime).FractionOf(BOUNCE_TIME);
                MapLoc = new Loc(AnimMath.Lerp(ToLoc.X * GraphicsManager.TileSize, RecoilLoc.X * GraphicsManager.TileSize, intb),
                    AnimMath.Lerp(ToLoc.Y * GraphicsManager.TileSize, RecoilLoc.Y * GraphicsManager.TileSize, intb));
                LocHeight = AnimMath.GetArc(GraphicsManager.TileSize / 2, FrameTick.FrameToTick(BOUNCE_TIME), (ActionTime - totalTime).Ticks);
            }
        }

        public override IEnumerable<VisionLoc> GetVisionLocs()
        {
            int dist = (FromLoc - ToLoc).Dist8();
            int totalTime = KNOCKBACK_TIME_PER_TILE * dist;
            if (ActionTime < totalTime)
            {
                float diff = (ActionTime).FractionOf(totalTime);
                if (diff > 0f)
                    yield return new VisionLoc(RecoilLoc, diff);
                if (diff < 1f)
                    yield return new VisionLoc(FromLoc, 1f - diff);
            }
            else
                yield return new VisionLoc(RecoilLoc, 1f);//or maybe the diff from ToLoc to RecoilLoc
        }
    }


    public class CharAnimThrown : RecoilingAnimation
    {
        public const int ANIM_TIME = 30;
        const int BOUNCE_TIME = 10;
        public override bool ActionPassed { get { return ActionDone; } }
        private int getActionTime()
        {
            int dist = (FromLoc - ToLoc).Dist8();
            int totalTime = ANIM_TIME;
            if (RecoilLoc != ToLoc)
                totalTime += BOUNCE_TIME;
            return totalTime;
        }
        public override bool ActionDone { get { return ActionTime >= getActionTime(); } }

        protected override int AnimFrameType { get { return GraphicsManager.HurtAction; } }
        protected override void UpdateFrameInternal()
        {
            int totalTime = ANIM_TIME;
            if (ActionTime < totalTime)
            {
                double intb = (double)ActionTime.FractionOf(totalTime);
                MapLoc = new Loc(AnimMath.Lerp(FromLoc.X * GraphicsManager.TileSize, ToLoc.X * GraphicsManager.TileSize, intb),
                    AnimMath.Lerp(FromLoc.Y * GraphicsManager.TileSize, ToLoc.Y * GraphicsManager.TileSize, intb));
                double maxDistance = Math.Sqrt(((FromLoc - ToLoc) * GraphicsManager.TileSize).DistSquared());
                LocHeight = AnimMath.GetArc(maxDistance / 2, ANIM_TIME, ActionTime.ToFrames());
            }
            else
            {
                double intb = (double)(ActionTime - totalTime).FractionOf(BOUNCE_TIME);
                MapLoc = new Loc(AnimMath.Lerp(ToLoc.X * GraphicsManager.TileSize, RecoilLoc.X * GraphicsManager.TileSize, intb),
                    AnimMath.Lerp(ToLoc.Y * GraphicsManager.TileSize, RecoilLoc.Y * GraphicsManager.TileSize, intb));
                LocHeight = AnimMath.GetArc(GraphicsManager.TileSize / 2, FrameTick.FrameToTick(BOUNCE_TIME), (ActionTime - totalTime).Ticks);
            }
        }
        public override IEnumerable<VisionLoc> GetVisionLocs()
        {
            if (ActionTime < ANIM_TIME)
            {
                float diff = ActionTime.FractionOf(ANIM_TIME);
                if (diff > 0f)
                    yield return new VisionLoc(ToLoc, diff);
                if (diff < 1f)
                    yield return new VisionLoc(FromLoc, 1f - diff);
            }
            else
                yield return new VisionLoc(RecoilLoc, 1f);//or maybe the diff from ToLoc to RecoilLoc
        }
    }
    
}
