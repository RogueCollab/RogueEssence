using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using RogueEssence.Dev;

namespace RogueEssence.Dungeon
{
    [Flags]
    public enum Alignment
    {
        None = 0,
        Self = 1,
        Friend = 2,
        Foe = 4
    }

    public enum TileAlignment
    {
        None,
        Wall,
        Any
    }

    public enum AttackCoverage
    {
        Front,
        FrontAndCorners,
        Wide,
        Around
    }

    public enum LineCoverage
    {
        Front,
        FrontAndCorners,
        Wide
    }

    [Serializable]
    public abstract class CharAction
    {
        public const int MAX_RANGE = 80;

        //this class will instantiate hitboxes by its own logic
        //hitboxes will therefore not be editor members and exist only in runtime
        //it also means that the hitboxes need to have methods assigned to them at creation time
        //as well as all GFX/sound effect data
        //also need to import owner, and hit classes; may lead to huge constructor arguments
        [NonSerialized]
        protected CharAnimation currentAnim;

        [Dev.NonEdited]
        public Loc CharLoc
        {
            get { return currentAnim.CharLoc; }
            set { currentAnim.CharLoc = value; }
        }

        [Dev.NonEdited]
        public Loc CharLocFrom { get { return currentAnim.CharLocFrom; } }

        [Dev.NonEdited]
        public Dir8 CharDir
        {
            get { return currentAnim.CharDir; }
            set { currentAnim.CharDir = value; }
        }
        public virtual bool ActionDone { get { return currentAnim.ActionDone; } }
        public virtual bool WantsToEnd() { return currentAnim.WantsToEnd(); }
        public Loc MapLoc { get { return currentAnim.MapLoc; } }
        public int LocHeight { get { return currentAnim.LocHeight; } }
        public bool MajorAction { get { return currentAnim.MajorAnim; } }
        public bool ActionPassed { get { return currentAnim.ActionPassed; } }
        public Loc DrawOffset { get { return currentAnim.DrawOffset; } }
        public IEnumerable<Loc> GetLocsVisible() { return currentAnim.GetLocsVisible(); }
        public IEnumerable<VisionLoc> GetVisionLocs() { return currentAnim.GetVisionLocs(); }

        public virtual void OnUpdate(FrameTick elapsedTime, MonsterID appearance, int movementSpeed) { currentAnim.Update(elapsedTime, appearance, movementSpeed); }
        public virtual void OnUpdateHitboxes(FrameTick elapsedTime) { }
        public virtual void UpdateFrame() { currentAnim.UpdateFrame(); }
        public void UpdateDrawEffects(HashSet<DrawEffect> drawEffects) { currentAnim.UpdateDrawEffects(drawEffects); }
        public virtual Loc GetActionPoint(CharSheet sheet, ActionPointType pointType) { return currentAnim.GetActionPoint(sheet, pointType); }
        public virtual void Draw(SpriteBatch spriteBatch, Loc offset, CharSheet sheet) { currentAnim.Draw(spriteBatch, offset, sheet); }
        public Loc GetDrawLoc(Loc offset, CharSheet sheet) { return currentAnim.GetDrawLoc(sheet, offset); }
        public void GetCurrentSprite(CharSheet sheet, out int anim, out int currentTime, out int currentFrame) { currentAnim.GetCurrentSprite(sheet, out anim, out currentTime, out currentFrame); }

        public void PickUpFrom(MonsterID appearance, CharAction prevAction)
        {
            currentAnim.PickUpFrom(appearance, prevAction.currentAnim);
        }
        public bool ProcessInterruptingAnim(CharAnimation newAnim)
        {
            return currentAnim.ProcessInterruptingAction(newAnim);
        }
    }


    [Serializable]
    public class EmptyCharAction : CharAction
    {
        //this action throws out no hitbox
        public EmptyCharAction(CharAnimation anim)
        {
            currentAnim = anim;
        }
    }


    [Serializable]
    public abstract class CombatAction : CharAction
    {
        [NonSerialized]
        public Loc HitOffset;

        public Alignment TargetAlignments;
        public FiniteEmitter TileEmitter;


        public List<BattleFX> PreActions;

        public BattleFX ActionFX;
        public int LagBehindTime;

        public CombatAction()
        {
            TileEmitter = new EmptyFiniteEmitter();

            PreActions = new List<BattleFX>();

            ActionFX = new BattleFX();
        }
        protected CombatAction(CombatAction other)
        {
            HitOffset = other.HitOffset;
            TargetAlignments = other.TargetAlignments;

            PreActions = new List<BattleFX>();
            foreach (BattleFX fx in other.PreActions)
                PreActions.Add(new BattleFX(fx));

            ActionFX = new BattleFX(other.ActionFX);
            LagBehindTime = other.LagBehindTime;
            TileEmitter = (FiniteEmitter)other.TileEmitter.Clone();
        }
        public abstract CombatAction Clone();
        protected void BeginStaticAction(Character owner, CharAction prevAction, CharAnimData animData, bool fallShort = false)
        {
            StaticCharAnimation anim = animData.GetCharAnim();
            anim.CharLoc = owner.CharLoc;
            anim.CharDir = owner.CharDir;
            anim.MajorAnim = true;
            anim.FallShort = fallShort;
            BeginAnim(owner.Appearance, anim, prevAction);
        }
        protected void BeginAnim(MonsterID appearance, CharAnimation charAnim, CharAction prevAction)
        {
            currentAnim = charAnim;
            PickUpFrom(appearance, prevAction);
        }
        public virtual void BeginAction(Character owner, CharAction prevAction) { }
        public IEnumerator<YieldInstruction> OnIntro(Character owner)
        {
            foreach (BattleFX fx in PreActions)
            {
                //play sound
                GameManager.Instance.BattleSE(fx.Sound);
                //the animation
                FiniteEmitter fxEmitter = (FiniteEmitter)fx.Emitter.Clone();
                fxEmitter.SetupEmit(owner.MapLoc, owner.MapLoc, owner.CharDir);
                DungeonScene.Instance.CreateAnim(fxEmitter, DrawLayer.NoDraw);
                DungeonScene.Instance.SetScreenShake(new ScreenMover(fx.ScreenMovement));
                yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(fx.Delay, owner.CharLoc));
            }
        }
        protected IEnumerator<YieldInstruction> PassEmitter(Character owner)
        {
            GameManager.Instance.BattleSE(ActionFX.Sound);
            FiniteEmitter emitter = (FiniteEmitter)ActionFX.Emitter.Clone();
            Loc origin = owner.MapLoc + HitOffset * GraphicsManager.TileSize;
            emitter.SetupEmit(origin, origin, owner.CharDir);
            DungeonScene.Instance.CreateAnim(emitter, DrawLayer.NoDraw);
            DungeonScene.Instance.SetScreenShake(new ScreenMover(ActionFX.ScreenMovement));
            yield return new WaitForFrames(GameManager.Instance.ModifyBattleSpeed(ActionFX.Delay, owner.CharLoc));
        }
        public abstract IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect);

        public abstract IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod);
        public abstract bool IsWide();
        public abstract int GetEffectiveDistance();
        public abstract int Distance { get; set; }
        public abstract string GetDescription();
        public string GetTargetsString(bool plural)
        {
            return GetTargetsString(plural, TargetAlignments);
        }
        public static string GetTargetsString(bool plural, Alignment alignment)
        {
            bool self = (alignment & Alignment.Self) == Alignment.Self;
            bool friends = (alignment & Alignment.Friend) == Alignment.Friend;
            bool foes = (alignment & Alignment.Foe) == Alignment.Foe;
            if (self && friends && foes)
                return Text.FormatKey("TARGET_HIT_ALL");
            else if (self && friends && !foes)
                return Text.FormatKey("TARGET_HIT_PARTY");
            else if (!self && friends && foes)
                return (plural ? Text.FormatKey("TARGET_HIT_OTHER_PLURAL") : Text.FormatKey("TARGET_HIT_OTHER"));
            else if (self && !friends && foes)
                return (plural ? Text.FormatKey("TARGET_HIT_USERFOE_PLURAL") : Text.FormatKey("TARGET_HIT_USERFOE"));
            else if (self && !friends && !foes)
                return Text.FormatKey("TARGET_HIT_USER");
            else if (!self && friends && !foes)
                return (plural ? Text.FormatKey("TARGET_HIT_ALLY_PLURAL") : Text.FormatKey("TARGET_HIT_ALLY"));
            else if (!self && !friends && foes)
                return (plural ? Text.FormatKey("TARGET_HIT_FOE_PLURAL") : Text.FormatKey("TARGET_HIT_FOE"));
            return Text.FormatKey("TARGET_HIT_NONE");
        }

        public override string ToString()
        {
            
            return base.ToString() + ": " + GetDescription();
        }
    }

    [Serializable]
    public abstract class CharAnimData
    {
        public abstract StaticCharAnimation GetCharAnim();
        public abstract CharAnimData Clone();
    }

    [Serializable]
    public class CharAnimFrameType : CharAnimData
    {
        [Dev.FrameType(0, false)]
        public int ActionType;

        public CharAnimFrameType() { }
        public CharAnimFrameType(int actionType) { ActionType = actionType; }
        protected CharAnimFrameType(CharAnimFrameType other)
        {
            ActionType = other.ActionType;
        }
        public override CharAnimData Clone() { return new CharAnimFrameType(this); }

        public override StaticCharAnimation GetCharAnim()
        {
            CharFrameType frameType = GraphicsManager.Actions[ActionType];
            if (frameType.IsDash)
            {
                CharLungeAction action = new CharLungeAction();
                action.BaseFrameType = ActionType;
                return action;
            }
            else
            {
                CharAnimAction action = new CharAnimAction();
                action.BaseFrameType = ActionType;
                return action;
            }
        }
    }

    [Serializable]
    public class CharAnimProcess : CharAnimData
    {
        public enum ProcessType
        {
            None,
            Spin,
            Drop,
            Fly
        }

        public ProcessType Process;

        public CharAnimProcess() { }
        public CharAnimProcess(ProcessType process) { Process = process; }
        protected CharAnimProcess(CharAnimProcess other)
        {
            Process = other.Process;
        }
        public override CharAnimData Clone() { return new CharAnimProcess(this); }

        public override StaticCharAnimation GetCharAnim()
        {
            switch (Process)
            {
                case ProcessType.Spin:
                    return new CharAnimSpin();
                case ProcessType.Drop:
                    return new CharAnimDrop();
                case ProcessType.Fly:
                    return new CharAnimFly();
                default:
                    return null;
            }
        }
    }


    [Serializable]
    public class SelfAction : CombatAction
    {
        public CharAnimData CharAnimData;

        //this action throws out no hitbox
        public SelfAction() { CharAnimData = new CharAnimFrameType(0); }
        protected SelfAction(SelfAction other)
            : base(other)
        {
            CharAnimData = other.CharAnimData.Clone();
        }
        public override CombatAction Clone() { return new SelfAction(this); }
        public override void BeginAction(Character owner, CharAction prevAction) { BeginStaticAction(owner, prevAction, CharAnimData); }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //simply add an animation
            //as well as call hit method
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            //no end tiles here
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User, new SelfHitbox(actionContext.User, TileEmitter, LagBehindTime),
                effect, tileEffect));
        }

        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            SelfHitbox hitbox = new SelfHitbox(owner, TileEmitter, LagBehindTime);
            hitbox.PreCalculateAllTargets();

            while (hitbox.TilesToHit.Count > 0)
            {
                Loc tile = hitbox.TilesToHit.Dequeue();
                if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                    yield return tile;
            }
        }
        public override bool IsWide()
        {
            return true;
        }
        public override int GetEffectiveDistance() { return 0; }
        public override int Distance { get { return 0; } set { } }

        public override string GetDescription()
        {
            return GetTargetsString(false);
        }
    }

    [Serializable]
    public class AttackAction : CombatAction
    {
        public bool HitTiles;
        public TileAlignment BurstTiles;
        public FiniteEmitter Emitter;

        //create one or more SingleFrameHitbox;

        public AttackCoverage WideAngle;

        public CharAnimData CharAnimData;

        [NonSerialized]
        public bool stopHitboxes;

        public AttackAction()
        {
            Emitter = new EmptyFiniteEmitter();
            CharAnimData = new CharAnimFrameType(0);
        }
        protected AttackAction(AttackAction other)
            : base(other)
        {
            HitTiles = other.HitTiles;
            BurstTiles = other.BurstTiles;
            WideAngle = other.WideAngle;
            CharAnimData = other.CharAnimData.Clone();
            Emitter = (FiniteEmitter)other.Emitter.Clone();
        }
        public override CombatAction Clone() { return new AttackAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction)
        {
            if (WideAngle == AttackCoverage.Front)
            {
                if (DungeonScene.Instance.ShotBlocked(owner, owner.CharLoc, owner.CharDir, Alignment.None, true, true, true))
                    stopHitboxes = true;
            }
            BeginStaticAction(owner, prevAction, CharAnimData, stopHitboxes);
        }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //create the single-frame hitbox(es) and toss them into the wild
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            List<Hitbox> hitboxes = new List<Hitbox>();
            if (WideAngle == AttackCoverage.Front)
            {
                if (DungeonScene.Instance.ShotBlocked(actionContext.User, actionContext.User.CharLoc + HitOffset, actionContext.User.CharDir, Alignment.None, true, true, true))
                    stopHitboxes = true;
            }
            Loc frontLoc = actionContext.User.CharLoc + HitOffset;
            if (!stopHitboxes)
            {
                frontLoc = frontLoc + actionContext.User.CharDir.GetLoc();
                hitboxes.Add(new StaticHitbox(actionContext.User, TargetAlignments, HitTiles, BurstTiles, frontLoc, TileEmitter, Emitter, LagBehindTime));
            }
            Loc leftLoc = actionContext.User.CharLoc + HitOffset;
            Loc rightLoc = actionContext.User.CharLoc + HitOffset;
            if (WideAngle >= AttackCoverage.Wide)
            {
                leftLoc = leftLoc + DirExt.AddAngles(actionContext.User.CharDir, Dir8.DownRight).GetLoc();
                hitboxes.Insert(0, new StaticHitbox(actionContext.User, TargetAlignments, HitTiles, BurstTiles, leftLoc, TileEmitter, Emitter, LagBehindTime));
                rightLoc = rightLoc + DirExt.AddAngles(actionContext.User.CharDir, Dir8.DownLeft).GetLoc();
                hitboxes.Add(new StaticHitbox(actionContext.User, TargetAlignments, HitTiles, BurstTiles, rightLoc, TileEmitter, Emitter, LagBehindTime));
            }
            Loc[] otherLocs = new Loc[5];
            for (int ii = 0; ii < otherLocs.Length; ii++)
                otherLocs[ii] = actionContext.User.CharLoc + HitOffset;
            if (WideAngle >= AttackCoverage.Around)
            {
                for (int ii = 0; ii < otherLocs.Length; ii++)
                {
                    otherLocs[ii] = otherLocs[ii] + DirExt.AddAngles(actionContext.User.CharDir, (Dir8)(ii + 2)).GetLoc();
                    hitboxes.Add(new StaticHitbox(actionContext.User, TargetAlignments, HitTiles, BurstTiles, otherLocs[ii], TileEmitter, Emitter, LagBehindTime));
                }
            }

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User, hitboxes,
                effect, tileEffect));
            if (!stopHitboxes)
                actionContext.StrikeLandTiles.Add(frontLoc);
            if (WideAngle >= AttackCoverage.Wide)
            {
                actionContext.StrikeLandTiles.Insert(0, leftLoc);
                actionContext.StrikeLandTiles.Add(rightLoc);
            }
            if (WideAngle >= AttackCoverage.Around)
            {
                for (int ii = 0; ii < otherLocs.Length; ii++)
                    actionContext.StrikeLandTiles.Add(otherLocs[ii]);
            }
        }

        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            List<Hitbox> hitboxes = new List<Hitbox>();
            bool releaseCenter = true;
            if (WideAngle == AttackCoverage.Front)
            {
                if (DungeonScene.Instance.ShotBlocked(owner, owner.CharLoc, dir, Alignment.None, true, true, true))
                    releaseCenter = false;
            }
            Loc frontLoc = owner.CharLoc;
            if (releaseCenter)
            {
                frontLoc = frontLoc + dir.GetLoc();
                hitboxes.Add(new StaticHitbox(owner, TargetAlignments, HitTiles, BurstTiles, frontLoc, TileEmitter, Emitter, LagBehindTime));
            }
            if (WideAngle >= AttackCoverage.Wide)
            {
                Loc leftLoc = owner.CharLoc + DirExt.AddAngles(dir, Dir8.DownRight).GetLoc();
                hitboxes.Insert(0, new StaticHitbox(owner, TargetAlignments, HitTiles, BurstTiles, leftLoc, TileEmitter, Emitter, LagBehindTime));
                Loc rightLoc = owner.CharLoc + DirExt.AddAngles(dir, Dir8.DownLeft).GetLoc();
                hitboxes.Add(new StaticHitbox(owner, TargetAlignments, HitTiles, BurstTiles, rightLoc, TileEmitter, Emitter, LagBehindTime));
            }
            if (WideAngle >= AttackCoverage.Around)
            {
                for (int ii = 0; ii < 5; ii++)
                {
                    Loc otherLoc = owner.CharLoc + DirExt.AddAngles(dir, (Dir8)(ii + 2)).GetLoc();
                    hitboxes.Add(new StaticHitbox(owner, TargetAlignments, HitTiles, BurstTiles, otherLoc, TileEmitter, Emitter, LagBehindTime));
                }
            }

            foreach (Hitbox hitbox in hitboxes)
            {
                hitbox.PreCalculateAllTargets();

                while (hitbox.TilesToHit.Count > 0)
                {
                    Loc tile = hitbox.TilesToHit.Dequeue();
                    if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                        yield return tile;
                }
            }
        }
        public override bool IsWide()
        {
            return WideAngle >= AttackCoverage.Wide;
        }
        public override int GetEffectiveDistance() { return 1; }
        public override int Distance { get { return 1; } set { } }

        public override string GetDescription()
        {
            switch (WideAngle)
            {
                case AttackCoverage.Around: return Text.FormatKey("RANGE_ATTACK_AROUND", GetTargetsString(true));
                case AttackCoverage.Wide: return Text.FormatKey("RANGE_ATTACK_WIDE", GetTargetsString(true));
                case AttackCoverage.FrontAndCorners: return Text.FormatKey("RANGE_ATTACK_CORNER", GetTargetsString(false));
                case AttackCoverage.Front: return Text.FormatKey("RANGE_ATTACK_FRONT", GetTargetsString(false));
                default: return "???";
            }
        }
    }

    [Serializable]
    public class AreaAction : CombatAction
    {
        public CircleSquareEmitter Emitter;

        //create a CircleSquareHitbox;
        public bool HitTiles;
        public TileAlignment BurstTiles;
        public Hitbox.AreaLimit HitArea;

        /// <summary>
        /// In Tiles
        /// </summary>
        public int Range;

        /// <summary>
        /// Speed to Spread from 0 to Range in Tiles Per Second
        /// </summary>
        public int Speed;

        public CharAnimData CharAnimData;

        public AreaAction()
        {
            Emitter = new EmptyCircleSquareEmitter();
            CharAnimData = new CharAnimFrameType(0);
        }
        protected AreaAction(AreaAction other)
            : base(other)
        {
            HitTiles = other.HitTiles;
            BurstTiles = other.BurstTiles;
            HitArea = other.HitArea;
            Range = other.Range;
            Speed = other.Speed;
            CharAnimData = other.CharAnimData.Clone();
            Emitter = (CircleSquareEmitter)other.Emitter.Clone();
        }
        public override CombatAction Clone() { return new AreaAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction) { BeginStaticAction(owner, prevAction, CharAnimData); }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //create the area hitbox(es) and toss them into the wild
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            Loc groundZero = actionContext.User.CharLoc + HitOffset;
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User,
                new CircleSquareHitbox(actionContext.User, TargetAlignments, HitTiles, BurstTiles, groundZero, TileEmitter, Emitter, GetModRange(TargetAlignments, 0), Speed, LagBehindTime, HitArea, actionContext.User.CharDir),
                effect, tileEffect));
        }

        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            Loc groundZero = owner.CharLoc;
            CircleSquareHitbox hitbox = new CircleSquareHitbox(owner, TargetAlignments, HitTiles, BurstTiles, groundZero, TileEmitter, Emitter, GetModRange(TargetAlignments, rangeMod), Speed, LagBehindTime, HitArea, dir);

            hitbox.PreCalculateAllTargets();

            while (hitbox.TilesToHit.Count > 0)
            {
                Loc tile = hitbox.TilesToHit.Dequeue();
                if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                    yield return tile;
            }
        }

        public static List<Character> GetTargetsInArea(Character user, Loc loc, Alignment targetAlignments, int range)
        {
            return GetTargetsInArea(user, loc, targetAlignments, range, Hitbox.AreaLimit.Full);
        }
        public static List<Character> GetTargetsInArea(Character user, Loc loc, Alignment targetAlignments, int range, Hitbox.AreaLimit hitArea)
        {
            List<Character> targets = new List<Character>();
            foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
            {
                if (IsTargetedInArea(user, loc, user.CharDir, targetAlignments, range, hitArea, character))
                    targets.Add(character);
            }
            return targets;
        }
        public static bool IsTargetedInArea(Character user, Loc loc, Dir8 dir, Alignment targetAlignments, int range, Hitbox.AreaLimit hitArea, Character target)
        {
            Loc diff = loc - target.CharLoc;
            if (DungeonScene.Instance.IsTargeted(user, target, targetAlignments) && Hitbox.IsInCircleSquareHitbox(target.CharLoc, loc, range * 2, range, hitArea, dir))
                return true;

            return false;
        }
        public override bool IsWide()
        {
            return true;
        }
        private int GetModRange(Alignment targetAlignments, int mod)
        {
            if ((targetAlignments & Alignment.Self) != Alignment.None || targetAlignments == Alignment.None)
                return Math.Max(0, Range + mod);
            else
                return Math.Max(1, Range + mod);
        }
        public override int GetEffectiveDistance()
        {
            if ((TargetAlignments & Alignment.Self) != Alignment.None || TargetAlignments == Alignment.None)
                return Math.Max(0, Range);
            else
                return Math.Max(1, Range);
        }
        public override int Distance { get { return Range; } set { Range = value; } }

        public override string GetDescription()
        {
            switch (HitArea)
            {
                case Hitbox.AreaLimit.Full:
                    return Text.FormatKey("RANGE_AREA_FULL", Range, GetTargetsString(true));
                case Hitbox.AreaLimit.Cone:
                    return Text.FormatKey("RANGE_AREA_CONE", Range, GetTargetsString(true));
                case Hitbox.AreaLimit.Sides:
                    return Text.FormatKey("RANGE_AREA_SIDES", Range, GetTargetsString(true));
                default:
                    return "???";
            }
        }
    }



    [Serializable]
    public class OffsetAction : CombatAction
    {
        public enum OffsetArea
        {
            Tile,
            Sides,
            Area
        }

        public CircleSquareEmitter Emitter;

        //create a CircleSquareHitbox;
        public bool HitTiles;
        public TileAlignment BurstTiles;
        public OffsetArea HitArea;

        /// <summary>
        /// In Tiles
        /// </summary>
        public int Range;

        /// <summary>
        /// Speed to Spread from 0 to Range in Tiles Per Second.  Use 0 for instant travel.
        /// </summary>
        public int Speed;

        public CharAnimData CharAnimData;

        public OffsetAction()
        {
            Emitter = new EmptyCircleSquareEmitter();
            CharAnimData = new CharAnimFrameType(0);
        }
        protected OffsetAction(OffsetAction other)
            : base(other)
        {
            HitTiles = other.HitTiles;
            BurstTiles = other.BurstTiles;
            HitArea = other.HitArea;
            Range = other.Range;
            Speed = other.Speed;
            CharAnimData = other.CharAnimData.Clone();
            Emitter = (CircleSquareEmitter)other.Emitter.Clone();
        }
        public override CombatAction Clone() { return new OffsetAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction) { BeginStaticAction(owner, prevAction, CharAnimData); }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //create the area hitbox(es) and toss them into the wild
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            Loc groundZero = GetLanding(actionContext.User.CharLoc, actionContext.User.CharDir, 0);
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User,
                new CircleSquareHitbox(actionContext.User, TargetAlignments, HitTiles, BurstTiles, groundZero, TileEmitter, Emitter,
                    (HitArea == OffsetArea.Tile) ? 0 : 1, Speed, LagBehindTime, (HitArea == OffsetArea.Sides) ? Hitbox.AreaLimit.Sides : Hitbox.AreaLimit.Full, actionContext.User.CharDir),
                effect, tileEffect));
            actionContext.StrikeLandTiles.Add(groundZero);
        }

        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            Loc groundZero = GetLanding(owner.CharLoc, dir, rangeMod);
            CircleSquareHitbox hitbox = new CircleSquareHitbox(owner, TargetAlignments, HitTiles, BurstTiles, groundZero, TileEmitter, Emitter,
                (HitArea == OffsetArea.Tile) ? 0 : 1, Speed, LagBehindTime, (HitArea == OffsetArea.Sides) ? Hitbox.AreaLimit.Sides : Hitbox.AreaLimit.Full, dir);

            hitbox.PreCalculateAllTargets();

            while (hitbox.TilesToHit.Count > 0)
            {
                Loc tile = hitbox.TilesToHit.Dequeue();
                if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                    yield return tile;
            }
        }

        private Loc GetLanding(Loc ownerLoc, Dir8 dir, int mod)
        {
            int modRange = GetModRange(mod);

            Loc targetLoc = ownerLoc;
            Loc addLoc = dir.GetLoc();
            for (int ii = 0; ii < modRange; ii++)
            {
                targetLoc += addLoc;
                if (ZoneManager.Instance.CurrentMap.TileBlocked(targetLoc, true))
                    break;
            }
            return targetLoc + HitOffset;
        }

        public override bool IsWide()
        {
            return (HitArea != OffsetArea.Tile);
        }
        private int GetModRange(int mod)
        {
            return Math.Max(0, Range + mod);
        }
        public override int GetEffectiveDistance() { return Math.Max(0, Range); }
        public override int Distance { get { return Range; } set { Range = value; } }

        public override string GetDescription()
        {
            string areaString = "";

            switch (HitArea)
            {
                case OffsetArea.Area:
                    areaString = Text.FormatKey("RANGE_OFFSET_AREA", Range, GetTargetsString(true));
                    break;
                case OffsetArea.Sides:
                    areaString = Text.FormatKey("RANGE_OFFSET_SIDES", Range, GetTargetsString(true));
                    break;
                case OffsetArea.Tile:
                    areaString = Text.FormatKey("RANGE_OFFSET_TILE", Range, GetTargetsString(false));
                    break;
            }

            return areaString;
        }
    }

    [Serializable]
    public abstract class LinearAction : CombatAction
    {
        public bool HitTiles;
        public int Range;

        /// <summary>
        /// When set, will stop at the first eligible target it hits.
        /// </summary>
        public bool StopAtHit;
        
        /// <summary>
        /// When set, will explode on a wall and stop there.  When unset, it will move past the wall and not explode.
        /// </summary>
        public bool StopAtWall;

        public LinearAction() { }
        protected LinearAction(LinearAction other)
            : base(other)
        {
            HitTiles = other.HitTiles;
            Range = other.Range;
            StopAtHit = other.StopAtHit;
            StopAtWall = other.StopAtWall;
        }

        protected int GetModifiedRange(Character owner, Loc ownerLoc, Alignment targetAlignments, Dir8 dir, int mod, bool useMobility)
        {
            int hitRange = 1;
            Loc testPoint = ownerLoc;
            for (; hitRange < GetModRange(mod); hitRange++)
            {
                if (DungeonScene.Instance.ShotBlocked(owner, testPoint, dir, StopAtHit ? targetAlignments : Alignment.None, useMobility, StopAtWall))
                    break;
                testPoint = testPoint + dir.GetLoc();
            }
            return hitRange;
        }
        protected int GetModRange(int mod)
        {
            return Math.Max(1, Range + mod);
        }
        public override int GetEffectiveDistance() { return Math.Max(1, Range); }
        public override int Distance { get { return Range; } set { Range = value; } }
    }

    [Serializable]
    public class ProjectileAction : LinearAction
    {
        public enum RayCount
        {
            One,
            Three,
            FourCross,
            Five,
            Eight
        }

        public AnimData Anim;
        public AttachPointEmitter Emitter;
        public ShootingEmitter StreamEmitter;

        public RayCount Rays;

        /// <summary>
        /// Tiles per Second.  Use 0 for instant travel.
        /// </summary>
        public int Speed;
        public bool Boomerang;

        [Anim(0, "Item/")]
        public string ItemSprite;

        public CharAnimData CharAnimData;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ItemSprite = "";
        }

        public ProjectileAction()
        {
            ItemSprite = "";
            Anim = new AnimData();
            Emitter = new EmptyAttachEmitter();
            StreamEmitter = new EmptyShootingEmitter();
            CharAnimData = new CharAnimFrameType(0);
        }
        protected ProjectileAction(ProjectileAction other)
            : base(other)
        {
            Rays = other.Rays;
            Speed = other.Speed;
            Boomerang = other.Boomerang;
            ItemSprite = other.ItemSprite;
            CharAnimData = other.CharAnimData.Clone();
            Anim = new AnimData(other.Anim);
            Emitter = (AttachPointEmitter)other.Emitter.Clone();
            StreamEmitter = (ShootingEmitter)other.StreamEmitter.Clone();
        }
        public override CombatAction Clone() { return new ProjectileAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction) { BeginStaticAction(owner, prevAction, CharAnimData); }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //create the projectile hitbox(es) and toss them into the wild
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));

            List<Hitbox> hitboxes = new List<Hitbox>();

            CreateHitbox(actionContext, actionContext.User.CharDir, hitboxes);

            if (Rays == RayCount.Three || Rays == RayCount.Eight)
            {
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.DownRight), hitboxes);
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.DownLeft), hitboxes);
            }
            if (Rays == RayCount.Five || Rays == RayCount.Eight || Rays == RayCount.FourCross)
            {
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.Right), hitboxes);
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.Left), hitboxes);
            }
            if (Rays == RayCount.Five || Rays == RayCount.Eight)
            {
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.UpRight), hitboxes);
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.UpLeft), hitboxes);
            }
            if (Rays == RayCount.Eight || Rays == RayCount.FourCross)
                CreateHitbox(actionContext, DirExt.AddAngles(actionContext.User.CharDir, Dir8.Up), hitboxes);

            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User, hitboxes,
                effect, tileEffect));
        }
        private void CreateHitbox(IActionContext actionContext, Dir8 testDir, List<Hitbox> hitboxes)
        {
            int modRange = GetRangeBlock(actionContext.User, actionContext.User.CharLoc + HitOffset, TargetAlignments, testDir, 0, actionContext.ActionType == BattleActionType.Throw);
            if (Speed > 0)
            {
                ShootingEmitter shotEmitter = (ShootingEmitter)StreamEmitter.Clone();
                shotEmitter.SetupEmit(actionContext.User.MapLoc + HitOffset * GraphicsManager.TileSize, testDir, modRange * GraphicsManager.TileSize, Speed * GraphicsManager.TileSize);
                DungeonScene.Instance.CreateAnim(shotEmitter, DrawLayer.NoDraw);
            }
            hitboxes.Add(new CircleSweepHitbox(actionContext.User, TargetAlignments, HitTiles, StopAtWall, actionContext.User.CharLoc + HitOffset, Anim, TileEmitter, Emitter, Speed, LagBehindTime, testDir, modRange, Boomerang, ItemSprite));

            Loc endPoint = actionContext.User.CharLoc + HitOffset + testDir.GetLoc() * modRange;
            actionContext.StrikeLandTiles.Add(endPoint);
        }
        private void CreateTestHitbox(Character owner, Dir8 testDir, int mod, List<Hitbox> hitboxes)
        {
            int modRange = GetRangeBlock(owner, owner.CharLoc, TargetAlignments, testDir, mod, false);
            hitboxes.Add(new CircleSweepHitbox(owner, TargetAlignments, HitTiles, StopAtWall, owner.CharLoc, Anim, TileEmitter, Emitter, Speed, LagBehindTime, testDir, modRange, Boomerang, ItemSprite));
        }
        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            List<Hitbox> hitboxes = new List<Hitbox>();

            CreateTestHitbox(owner, dir, rangeMod, hitboxes);

            if (Rays == RayCount.Three || Rays == RayCount.Eight)
            {
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.DownRight), rangeMod, hitboxes);
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.DownLeft), rangeMod, hitboxes);
            }
            if (Rays == RayCount.Five || Rays == RayCount.Eight)
            {
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.Right), rangeMod, hitboxes);
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.Left), rangeMod, hitboxes);
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.UpRight), rangeMod, hitboxes);
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.UpLeft), rangeMod, hitboxes);
            }
            if (Rays == RayCount.Eight)
                CreateTestHitbox(owner, DirExt.AddAngles(dir, Dir8.Up), rangeMod, hitboxes);

            foreach (Hitbox hitbox in hitboxes)
            {
                hitbox.PreCalculateAllTargets();

                while (hitbox.TilesToHit.Count > 0)
                {
                    Loc tile = hitbox.TilesToHit.Dequeue();
                    if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                        yield return tile;
                }
            }
        }



        private int GetRangeBlock(Character owner, Loc ownerLoc, Alignment targetAlignments, Dir8 dir, int mod, bool throwing)
        {
            if (!throwing || StopAtHit)
                return GetModifiedRange(owner, ownerLoc, targetAlignments, dir, mod, false);
            else
            {//only reach this block if the attack is *supposed* to pierce, and is the result of a throw
                int hitRange = 1;
                Loc testPoint = ownerLoc;
                for (; hitRange < GetModRange(mod); hitRange++)
                {
                    if (DungeonScene.Instance.ShotBlocked(owner, testPoint, dir, StopAtHit ? targetAlignments : Alignment.None, false, StopAtWall))
                        break;

                    //additional check; at this time we are potentially piercing an enemy that cannot be pierced.
                    bool hitStop = false;
                    foreach (Character target in ZoneManager.Instance.CurrentMap.IterateCharacters())
                    {
                        if (DungeonScene.Instance.IsTargeted(owner, target, targetAlignments) && target.CharLoc == testPoint)
                        {
                            if (target.StopItemAtHit)
                            {
                                hitStop = true;
                                break;
                            }
                        }
                    }
                    if (hitStop)
                        break;
                    testPoint = testPoint + dir.GetLoc();
                }
                return hitRange;
            }
        }

        public override bool IsWide()
        {
            return false;
        }

        public override string GetDescription()
        {
            string line = Text.FormatKey("RANGE_PROJECTILE");
            if (Rays != RayCount.One)
                line = Text.FormatKey("RANGE_PROJECTILE_SPREAD", line);
            if (!StopAtWall)
                line = Text.FormatKey("RANGE_LINEAR_PHASE", line);
            else if (!StopAtHit)
                line = Text.FormatKey("RANGE_LINEAR_PIERCE", line);
            return Text.FormatKey("RANGE_LINEAR", line, Range, GetTargetsString((Rays != RayCount.One || !StopAtHit) ? true : false));
        }
    }


    [Serializable]
    public class WaveMotionAction : LinearAction
    {
        //create a BeamSweepHitbox;

        public BeamAnimData Anim;
        public bool Wide;

        /// <summary>
        /// Tiles per Second.  Use 0 for instant travel.
        /// </summary>
        public int Speed;
        public int Linger;

        public CharAnimData CharAnimData;

        public WaveMotionAction()
        {
            Anim = new BeamAnimData();
            CharAnimData = new CharAnimFrameType(0);
        }
        protected WaveMotionAction(WaveMotionAction other)
            : base(other)
        {
            Anim = new BeamAnimData(other.Anim);
            Wide = other.Wide;
            Speed = other.Speed;
            CharAnimData = other.CharAnimData.Clone();
            Linger = other.Linger;
        }
        public override CombatAction Clone() { return new WaveMotionAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction) { BeginStaticAction(owner, prevAction, CharAnimData); }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //create the beam hitbox and toss it into the wild
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            int modRange = GetModifiedRange(actionContext.User, actionContext.User.CharLoc + HitOffset, TargetAlignments, actionContext.User.CharDir, 0, false);
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User,
                new BeamSweepHitbox(actionContext.User, TargetAlignments, HitTiles, StopAtWall, actionContext.User.CharLoc + HitOffset, Anim, TileEmitter, Speed, LagBehindTime, actionContext.User.CharDir, modRange, Wide, Linger),
                effect, tileEffect));
            Loc endPoint = actionContext.User.CharLoc + HitOffset + actionContext.User.CharDir.GetLoc() * modRange;
            actionContext.StrikeLandTiles.Add(endPoint);
        }
        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            BeamSweepHitbox hitbox = new BeamSweepHitbox(owner, TargetAlignments, HitTiles, StopAtWall, owner.CharLoc, Anim, TileEmitter, Speed, LagBehindTime, dir, GetModifiedRange(owner, owner.CharLoc, TargetAlignments, dir, rangeMod, false), Wide, Linger);

            hitbox.PreCalculateAllTargets();

            while (hitbox.TilesToHit.Count > 0)
            {
                Loc tile = hitbox.TilesToHit.Dequeue();
                if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                    yield return tile;
            }
        }
        public override bool IsWide()
        {
            return Wide;
        }

        public override string GetDescription()
        {
            string line = Text.FormatKey("RANGE_BEAM");
            //if (IsWide())
            //    line = Text.FormatKey("RANGE_LINEAR_WIDE", line);
            //if (!StopAtHit)
            //    line = Text.FormatKey("RANGE_LINEAR_PIERCE", line);
            //no beams aren't non-wide or non-piercing
            string main = Text.FormatKey("RANGE_LINEAR", line, Range, GetTargetsString(true));
            return StopAtWall ? main : Text.FormatKey("RANGE_LINEAR_PASS", main);
        }
    }

    [Serializable]
    public class ThrowAction : CombatAction
    {
        public enum ArcCoverage
        {
            Single,
            Line,
            WideAngle
        }

        public AnimData Anim;
        public AttachPointEmitter Emitter;

        /// <summary>
        /// In Tiles
        /// </summary>
        public int Range;


        /// <summary>
        /// Tiles per Second.  Use 0 for instant travel.
        /// </summary>
        public int Speed;
        public ArcCoverage Coverage;

        [Anim(0, "Item/")]
        public string ItemSprite;

        public CharAnimData CharAnimData;


        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ItemSprite = "";
        }

        public ThrowAction()
        {
            Anim = new AnimData();
            Emitter = new EmptyAttachEmitter();
            ItemSprite = "";
            CharAnimData = new CharAnimFrameType(0);
        }
        protected ThrowAction(ThrowAction other)
            : base(other)
        {
            Speed = other.Speed;
            Range = other.Range;
            Coverage = other.Coverage;
            ItemSprite = other.ItemSprite;
            CharAnimData = other.CharAnimData.Clone();
            Anim = new AnimData(other.Anim);
            Emitter = (AttachPointEmitter)other.Emitter.Clone();
        }
        public override CombatAction Clone() { return new ThrowAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction) { BeginStaticAction(owner, prevAction, CharAnimData); }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            //create the throw hitbox and toss it into the wild
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            Loc landing = GetLanding(actionContext.User, actionContext.User.CharLoc + HitOffset, actionContext.User.CharDir, 0);
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User,
                new ArcingHitbox(actionContext.User, actionContext.User.CharLoc + HitOffset, Anim, TileEmitter, Emitter, landing, Speed, ItemSprite, LagBehindTime),
                effect, tileEffect));
            actionContext.StrikeLandTiles.Add(landing);
        }
        private Loc GetLanding(Character owner, Loc ownerLoc, Dir8 dir, int mod)
        {
            int modRange = GetModRange(mod);
            //find the closest target to land on
            if (Coverage != ArcCoverage.Single)
            {
                List<Character> targets = new List<Character>();
                foreach (Character character in ZoneManager.Instance.CurrentMap.IterateCharacters())
                {
                    if (DungeonScene.Instance.IsTargeted(owner, character, TargetAlignments))
                        targets.Add(character);
                }
                Character target = GetTarget(owner, ownerLoc, dir, Coverage == ArcCoverage.WideAngle, modRange, targets);
                if (target != null)
                    return target.CharLoc;
            }

            //if impossible to find, use the default farthest landing spot
            Loc targetLoc = ownerLoc;
            Loc addLoc = dir.GetLoc();
            for (int ii = 0; ii < modRange; ii++)
            {
                targetLoc += addLoc;
                if (ZoneManager.Instance.CurrentMap.TileBlocked(targetLoc, true))
                    break;
            }
            return targetLoc;
        }
        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            ArcingHitbox hitbox = new ArcingHitbox(owner, owner.CharLoc, Anim, TileEmitter, Emitter, GetLanding(owner, owner.CharLoc, dir, rangeMod), Speed, ItemSprite, LagBehindTime);

            hitbox.PreCalculateAllTargets();

            while (hitbox.TilesToHit.Count > 0)
            {
                Loc tile = hitbox.TilesToHit.Dequeue();
                if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                    yield return tile;
            }
        }
        public override bool IsWide()
        {
            return false;
        }


        public static Character GetTarget(Character owner, Loc ownerLoc, Dir8 dir, bool wide, int range, IEnumerable<Character> targets)
        {
            Loc targetLoc = ownerLoc;
            List<bool> sideL = new List<bool>();
            List<bool> sideR = new List<bool>();
            bool sideM = true;
            for (int front = 0; front < range; front++)
            {
                targetLoc = targetLoc + dir.GetLoc();

                if (sideM)
                {
                    //check directly forward
                    foreach (Character character in targets)
                    {
                        if (targetLoc == character.CharLoc)
                            return character;
                    }
                }

                if (wide)
                {
                    Loc leftLoc = targetLoc;
                    Loc rightLoc = targetLoc;

                    if (front == 1)
                    {
                        sideL.Add(sideM);
                        sideR.Add(sideM);
                    }
                    else if (front > 1)
                    {
                        sideL.Add(sideL[sideL.Count - 1]);
                        sideR.Add(sideR[sideR.Count - 1]);
                    }

                    for (int side = 0; side < front; side++)
                    {
                        if (dir.IsDiagonal())
                        {
                            leftLoc = leftLoc + DirExt.AddAngles(dir, Dir8.UpRight).GetLoc();
                            rightLoc = rightLoc + DirExt.AddAngles(dir, Dir8.UpLeft).GetLoc();
                        }
                        else
                        {
                            leftLoc = leftLoc + DirExt.AddAngles(dir, Dir8.Right).GetLoc();
                            rightLoc = rightLoc + DirExt.AddAngles(dir, Dir8.Left).GetLoc();
                        }

                        if (sideL[side])
                        {
                            //check sides
                            foreach (Character character in targets)
                            {
                                if (leftLoc == character.CharLoc)
                                    return character;
                            }
                        }
                        if (sideR[side])
                        {
                            foreach (Character character in targets)
                            {
                                if (rightLoc == character.CharLoc)
                                    return character;
                            }
                        }


                        if (ZoneManager.Instance.CurrentMap.TileBlocked(leftLoc, true))
                            sideL[side] = false;
                        if (ZoneManager.Instance.CurrentMap.TileBlocked(rightLoc, true))
                            sideR[side] = false;
                    }
                }

                if (ZoneManager.Instance.CurrentMap.TileBlocked(targetLoc, true))
                    sideM = false;
            }
            return null;
        }

        private int GetModRange(int mod)
        {
            return Math.Max(1, Range + mod);
        }
        public override int GetEffectiveDistance() { return Math.Max(1, Range); }
        public override int Distance { get { return Range; } set { Range = value; } }

        public override string GetDescription()
        {
            if (Coverage == ArcCoverage.Single)
                return Text.FormatKey("RANGE_ARC_SINGLE", Range, GetTargetsString(false));
            else if (Coverage == ArcCoverage.Line)
                return Text.FormatKey("RANGE_ARC_LINE", Range, GetTargetsString(false));
            else if (Coverage == ArcCoverage.WideAngle)
                return Text.FormatKey("RANGE_ARC_WIDE", Range, GetTargetsString(false));
            return "???";
        }
    }

    [Serializable]
    public class DashAction : LinearAction
    {
        public enum DashAppearance
        {
            Normal,
            DropDown,
            Invisible
        }

        public AnimData Anim;
        public int AnimOffset;
        public AttachPointEmitter Emitter;

        public LineCoverage WideAngle;

        [Dev.FrameType(0, true)]
        public int CharAnim;


        public DashAppearance AppearanceMod;

        [NonSerialized]
        private List<Hitbox> hitboxes;
        [NonSerialized]
        private int modRange;
        [NonSerialized]
        private bool finishedLunge;
        [NonSerialized]
        private Loc startLoc;

        public override bool ActionDone { get { return base.ActionDone && hitboxes.Count == 0; } }
        public override bool WantsToEnd() { return base.WantsToEnd() && hitboxes.Count == 0; }

        public DashAction()
        {
            hitboxes = new List<Hitbox>();
            Anim = new AnimData();
            Emitter = new EmptyAttachEmitter();
        }
        protected DashAction(DashAction other)
            : base(other)
        {
            hitboxes = new List<Hitbox>();
            Anim = new AnimData(other.Anim);
            AnimOffset = other.AnimOffset;
            Emitter = (AttachPointEmitter)other.Emitter.Clone();
            WideAngle = other.WideAngle;
            CharAnim = other.CharAnim;
            AppearanceMod = other.AppearanceMod;
        }
        public override CombatAction Clone() { return new DashAction(this); }

        public override void BeginAction(Character owner, CharAction prevAction)
        {
            //set the player's action;
            bool blockedOff;
            GetRangeBlock(owner, TargetAlignments, owner.CharDir, 0, WideAngle == LineCoverage.Front, out modRange, out blockedOff);

            startLoc = owner.CharLoc;
            DashAnimation rushAnim = null;
            switch (AppearanceMod)
            {
                case DashAppearance.Normal:
                    {
                        rushAnim = new CharAnimRush();
                        break;
                    }
                case DashAppearance.DropDown:
                    {
                        rushAnim = new CharAnimDropDash();
                        break;
                    }
                case DashAppearance.Invisible:
                    {
                        rushAnim = new CharAnimGhostDash();
                        break;
                    }
            }

            rushAnim.BaseFrameType = CharAnim;

            rushAnim.Overshoot = blockedOff;
            rushAnim.FromLoc = startLoc;
            rushAnim.CharDir = owner.CharDir;

            Loc endLoc = startLoc + owner.CharDir.GetLoc() * modRange;
            rushAnim.ToLoc = endLoc;

            if (!owner.CantWalk)
            {
                bool stay = true;
                if (endLoc != startLoc && ZoneManager.Instance.CurrentMap.IsBlocked(endLoc, owner.Mobility))
                    stay = false;

                if (!stay)
                {
                    endLoc = endLoc - owner.CharDir.GetLoc();
                    while (ZoneManager.Instance.CurrentMap.IsBlocked(endLoc, owner.Mobility) && endLoc != startLoc)
                        endLoc = endLoc - owner.CharDir.GetLoc();
                }
                rushAnim.RecoilLoc = endLoc;
            }
            else
                rushAnim.RecoilLoc = startLoc;

            rushAnim.OnFinishLunge = OnFinishLunge;
            rushAnim.MajorAnim = true;

            BeginAnim(owner.Appearance, rushAnim, prevAction);
            base.BeginAction(owner, prevAction);
        }
        public override IEnumerator<YieldInstruction> ReleaseHitboxes(IActionContext actionContext, DungeonScene.HitboxEffect effect, DungeonScene.HitboxEffect tileEffect)
        {
            yield return CoroutineManager.Instance.StartCoroutine(PassEmitter(actionContext.User));
            //needs to release hitbox into its own hitbox list
            actionContext.StrikeEndTile = actionContext.User.CharLoc;

            List<Hitbox> newHitboxes = new List<Hitbox>();
            AttachedCircleHitbox hitbox = new AttachedCircleHitbox(actionContext.User, TargetAlignments, HitTiles, StopAtWall, startLoc, Anim, AnimOffset, TileEmitter, Emitter,
                modRange, currentAnim.AnimHitTime - currentAnim.AnimRushTime, actionContext.User.CharDir, WideAngle == LineCoverage.Wide, LagBehindTime);
            hitbox.HitboxDone = finishedLunge;
            newHitboxes.Add(hitbox);
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.ReleaseHitboxes(actionContext.User, newHitboxes, hitboxes, effect, tileEffect));
            Loc endPoint = actionContext.User.CharLoc;
            actionContext.StrikeLandTiles.Add(endPoint);
        }
        public void OnFinishLunge()
        {
            finishedLunge = true;
        }
        public override void OnUpdateHitboxes(FrameTick elapsedTime)
        {
            //update hitbox and check collisions
            foreach (AttachedCircleHitbox hitbox in hitboxes)
                hitbox.Update(elapsedTime);

            if (finishedLunge)
            {
                for (int ii = hitboxes.Count - 1; ii >= 0; ii--)
                {
                    ((AttachedCircleHitbox)hitboxes[ii]).HitboxDone = true;
                    if (hitboxes[ii].Finished)
                        hitboxes.RemoveAt(ii);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Loc offset, CharSheet sheet)
        {
            base.Draw(spriteBatch, offset, sheet);
            foreach (Hitbox hitbox in hitboxes)
                hitbox.Draw(spriteBatch, offset);
        }
        public override IEnumerable<Loc> GetPreTargets(Character owner, Dir8 dir, int rangeMod)
        {
            int newRange;
            bool blockedOff;
            GetRangeBlock(owner, TargetAlignments, dir, rangeMod, WideAngle == LineCoverage.Front, out newRange, out blockedOff);
            AttachedCircleHitbox hitbox = new AttachedCircleHitbox(owner, TargetAlignments, HitTiles, StopAtWall, owner.CharLoc, Anim, AnimOffset, TileEmitter, Emitter,
                newRange, 0, dir, WideAngle == LineCoverage.Wide, LagBehindTime);

            hitbox.PreCalculateAllTargets();

            while (hitbox.TilesToHit.Count > 0)
            {
                Loc tile = hitbox.TilesToHit.Dequeue();
                if (hitbox.IsValidTileTarget(tile) != Hitbox.TargetHitType.None)
                    yield return tile;
            }
        }

        private void GetRangeBlock(Character owner, Alignment targetAlignments, Dir8 dir, int mod, bool blockedDiagonal, out int newRange, out bool blocked)
        {
            blocked = false;
            if (!blockedDiagonal)
                newRange = GetModifiedRange(owner, owner.CharLoc, targetAlignments, dir, mod, true);
            else
            {
                newRange = 0;
                Loc testPoint = owner.CharLoc;
                for (; newRange < GetModRange(mod);)
                {
                    if (DungeonScene.Instance.ShotBlocked(owner, testPoint, dir, Alignment.None, true, StopAtWall, blockedDiagonal))
                    {
                        blocked = true;
                        break;
                    }
                    newRange++;
                    testPoint = testPoint + dir.GetLoc();

                    if (StopAtHit && DungeonScene.Instance.BlockedByCharacter(owner, testPoint, targetAlignments))
                        break;
                }
            }
        }

        public override bool IsWide()
        {
            return WideAngle == LineCoverage.Wide;
        }

        public override string GetDescription()
        {
            string line = Text.FormatKey("RANGE_DASH");
            if (IsWide())
                line = Text.FormatKey("RANGE_LINEAR_WIDE", line);
            if (!StopAtWall)
                line = Text.FormatKey("RANGE_LINEAR_PHASE", line);
            else if (!StopAtHit)
                line = Text.FormatKey("RANGE_LINEAR_PIERCE", line);
            return Text.FormatKey("RANGE_LINEAR", line, Range, GetTargetsString((IsWide() || !StopAtHit) ? true : false));
        }
    }

    //"wide" variables in AttackAction and DashAction into a coverage enum: Wide, FrontCorners, Front
    //Wide and FrontCorners will act as they used to
    //front will hit tiles in the cardinal directions if not blocked, and will hit tiles in diagonal directions if not blocked or diagonal-blocked
    //Front will not check the tile in front if it is blocked off at a corner
    //front for a dash will stop and not check the tile that has corners obscuring it
    //this means that if an action is set to explode, and set to explode on blocks, and it is set to "front"
    //then it will not be able to hit any blocks to explode on (it might still hit enemies and explode on them)
    //this means that if you want to explode on walls, you must at least be a corner-cutting attack
    //this also means that if you want to make a wall-smashing attack, you must be a corner-cutting attack
    //wall-smashing attacks that cannot cut corners are impossible in this setup
    //need to create another coverage: FrontAndHitWallsOnlyIfNonDiagonal
    //this will always hit tiles in the cardinal directions, and will hit tiles in diagonal directions if not diagonal-blocked
    //may need an error message for being blocked on a diagonal?

    //for dashing, turning off the StopAtBlock will render checkdiagonal irrelevant
    //when getting mod range for diagonal-blocked attacks, it's not just passing in a diagonal bool:
    //it is also changing the range to go one tile shorter when blocked
}
