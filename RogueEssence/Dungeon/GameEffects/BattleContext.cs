using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using System;

namespace RogueEssence.Dungeon
{
    public enum BattleActionType
    {
        None = -1,
        Skill,
        Item,
        Throw,
        Trap
    }

    public class BattleContext : GameContext, IActionContext
    {
        public const int DEFAULT_ATTACK_SLOT = -1;
        public const int FAKE_ATTACK_SLOT = -2;
        public const int EQUIP_ITEM_SLOT = -1;
        public const int FLOOR_ITEM_SLOT = -2;
        public const int FORCED_SLOT = -3;

        public Loc StrikeStartTile { get; set; }//the tile of the user before it started a strike (used for tipper effects)
        public Loc StrikeEndTile { get; set; }//the tile of the user JUST AFTER it started a strike (used for updating position)
        public Dir8 StartDir { get; set; }//the direcion of the user before it started a strike (used for multistrike confusion)
        public Loc ExplosionTile { get; set; }
        public Loc TargetTile { get; set; }
        public List<Loc> StrikeLandTiles { get; set; }//all tiles in which a strike's hitbox ended (used for item landing)


        public BattleActionType ActionType { get; set; }
        public int UsageSlot;
        public int StrikesMade;//current strikes
        public int Strikes;//total strikes
        public CombatAction HitboxAction { get; set; }
        public ExplosionData Explosion { get; set; }
        public BattleData Data { get; set; }
        public InvItem Item;//the item that is used, and most likely dropped
        public int SkillUsedUp;//the skill whose last charge was used up
        public AbortStatus TurnCancel;

        public string actionMsg;

        public bool Hit;
        public int RangeMod;

        public StateCollection<ContextState> GlobalContextStates;


        public BattleContext(BattleActionType actionType) : base()
        {
            TurnCancel = new AbortStatus();
            this.ActionType = actionType;
            UsageSlot = BattleContext.DEFAULT_ATTACK_SLOT;
            SkillUsedUp = -1;
            StrikeLandTiles = new List<Loc>();
            actionMsg = "";
            GlobalContextStates = new StateCollection<ContextState>();
        }

        public BattleContext(BattleContext other, bool copyGlobal) : base(other)
        {
            TurnCancel = other.TurnCancel;
            if (copyGlobal)
                GlobalContextStates = other.GlobalContextStates.Clone();
            else
                GlobalContextStates = other.GlobalContextStates;
            StrikeStartTile = other.StrikeStartTile;
            StrikeEndTile = other.StrikeEndTile;
            StartDir = other.StartDir;
            ExplosionTile = other.ExplosionTile;
            TargetTile = other.TargetTile;
            if (copyGlobal)
            {
                StrikeLandTiles = new List<Loc>();
                StrikeLandTiles.AddRange(other.StrikeLandTiles);
            }
            else
                StrikeLandTiles = other.StrikeLandTiles;
            ActionType = other.ActionType;
            UsageSlot = other.UsageSlot;
            StrikesMade = other.StrikesMade;
            Strikes = other.Strikes;
            HitboxAction = other.HitboxAction.Clone();
            Explosion = new ExplosionData(other.Explosion);
            Data = new BattleData(other.Data);
            Item = new InvItem(other.Item);
            SkillUsedUp = other.SkillUsedUp;
            actionMsg = other.actionMsg;
            Hit = other.Hit;
            RangeMod = other.RangeMod;
        }


        public int GetContextStateInt<T>(bool global, int defaultVal) where T : ContextIntState
        {
            ContextIntState countState = global ? GlobalContextStates.GetWithDefault<T>() : ContextStates.GetWithDefault<T>();
            if (countState == null)
                return defaultVal;
            else
                return countState.Count;
        }


        public void AddContextStateInt<T>(bool global, int addedVal) where T : ContextIntState
        {
            ContextIntState countState = global ? GlobalContextStates.GetWithDefault<T>() : ContextStates.GetWithDefault<T>();
            if (countState == null)
            {
                T newCount = (T)Activator.CreateInstance(typeof(T));
                newCount.Count = addedVal;
                if (global)
                    GlobalContextStates.Set(newCount);
                else
                    ContextStates.Set(newCount);
            }
            else
                countState.Count += addedVal;
        }

        public Multiplier GetContextStateMult<T>() where T : ContextMultState
        {
            return GetContextStateMult<T>(false, new Multiplier(1, 1));
        }
        public Multiplier GetContextStateMult<T>(bool global, Multiplier defaultVal) where T : ContextMultState
        {
            ContextMultState countState = global ? GlobalContextStates.GetWithDefault<T>() : ContextStates.GetWithDefault<T>();
            if (countState == null)
                return defaultVal;
            else
                return countState.Mult;
        }

        public void AddContextStateMult<T>(bool global, int numerator, int denominator) where T : ContextMultState
        {
            ContextMultState multState = global ? GlobalContextStates.GetWithDefault<T>() : ContextStates.GetWithDefault<T>();
            if (multState == null)
            {
                T newMult = (T)Activator.CreateInstance(typeof(T));
                newMult.Mult = new Multiplier(numerator, denominator);
                if (global)
                    GlobalContextStates.Set(newMult);
                else
                    ContextStates.Set(newMult);
            }
            else
                multState.Mult.AddMultiplier(numerator, denominator);
        }



        public IEnumerator<YieldInstruction> TargetTileWithExplosion(Loc target)
        {
            BattleContext context = new BattleContext(this, false);

            context.ExplosionTile = target;

            //pre-explosion check
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.BeforeExplosion(context));

            //release explosion
            yield return CoroutineManager.Instance.StartCoroutine(context.Explosion.ReleaseExplosion(context.ExplosionTile, context.User, ProcessHitLoc, ProcessHitTile));
        }

        public IEnumerator<YieldInstruction> ProcessHitLoc(Loc loc)
        {
            BattleContext actionContext = new BattleContext(this, false);

            Character charTarget = ZoneManager.Instance.CurrentMap.GetCharAtLoc(loc);
            actionContext.TargetTile = loc;
            if (charTarget != null && DungeonScene.Instance.IsTargeted(actionContext.User, charTarget, actionContext.Explosion.TargetAlignments))
            {
                actionContext.Target = charTarget;
                yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.Instance.HitTarget(actionContext, charTarget));//hit the character
            }
            //do thing to tile
            yield return CoroutineManager.Instance.StartCoroutine(actionContext.User.HitTile(actionContext));
        }

        public IEnumerator<YieldInstruction> ProcessHitTile(Loc loc)
        {
            BattleContext actionContext = new BattleContext(this, false);
            actionContext.TargetTile = loc;

            //do thing to tile
            yield return CoroutineManager.Instance.StartCoroutine(actionContext.User.HitTile(actionContext));
        }
    }
}
