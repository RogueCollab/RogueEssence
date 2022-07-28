using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueEssence.Dev;
using Newtonsoft.Json;

namespace RogueEssence.Data
{

    [Serializable]
    public class BattleData : GameEventOwner
    {

        public enum SkillCategory
        {
            None = 0,
            Physical,
            Magical,
            Status
        }

        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.Skill;
        }

        public override string GetID() { return ID; }
        //TODO: make this more nuanced for skills, traps, item usages and throws?
        public override string GetDisplayName()
        {
            if (DataType == DataManager.DataType.Skill)
                return DataManager.Instance.GetSkill(ID).GetIconName();
            else
            {
                EntrySummary summary = DataManager.Instance.DataIndices[DataType].Entries[ID];
                return summary.GetColoredName();
            }
        }

        public override string ToString()
        {
            ElementData element = DataManager.Instance.GetElement(Element);
            string type = Text.FormatKey("MENU_SKILLS_ELEMENT", element.Name.ToLocal());
            string category = Text.FormatKey("MENU_SKILLS_CATEGORY", Category.ToLocal());
            BasePowerState powerState = SkillStates.GetWithDefault<BasePowerState>();
            string power = Text.FormatKey("MENU_SKILLS_POWER", (powerState != null ? powerState.Power.ToString() : "---"));
            string acc = Text.FormatKey("MENU_SKILLS_HIT_RATE", (HitRate > -1 ? HitRate + "%" : "---"));
            return type + ",  " + category + "\n" + power + ", " + acc;
        }

        [NonSerialized]
        public string ID;

        [NonSerialized]
        public DataManager.DataType DataType;

        [JsonConverter(typeof(ElementConverter))]
        [DataType(0, DataManager.DataType.Element, false)]
        public string Element;

        public SkillCategory Category;

        /// <summary>
        /// The chance of the attack hitting.
        /// </summary>
        public int HitRate;

        /// <summary>
        /// Special variables that this skill contains.
        /// They are potentially checked against in a select number of battle events.
        /// </summary>
        public StateCollection<SkillState> SkillStates;

        /// <summary>
        /// Occurs before the attacker tries to use the skill.
        /// If the skill is cancelled here, the turn and skill are not used.
        /// </summary>
        public PriorityList<BattleEvent> BeforeTryActions;

        /// <summary>
        /// Occurs before the attacker uses the skill.
        /// If the skill is cancelled here, the turn will still be passed.
        /// </summary>
        public PriorityList<BattleEvent> BeforeActions;

        /// <summary>
        /// Occurs right after the attacker uses the skill.
        /// The skill will have been called out, and the turn will be passed.
        /// In a skill with multiple strikes, this event will be called at the beginning of each strike.
        /// </summary>
        public PriorityList<BattleEvent> OnActions;

        /// <summary>
        /// Occurs after a tile is targeted and before it creates a splash damage hitbox.
        /// Can be used to alter the hitbox or redirect it.
        /// </summary>
        public PriorityList<BattleEvent> BeforeExplosions;

        /// <summary>
        /// Occurs before the target is hit.
        /// At this point, the target variable is available for calculations.
        /// </summary>
        public PriorityList<BattleEvent> BeforeHits;

        /// <summary>
        /// Occurs when the target is hit.
        /// Does not occur if the target evaded the attack.
        /// </summary>
        public PriorityList<BattleEvent> OnHits;

        /// <summary>
        /// Occurs when the attack hits a tile.
        /// Can be used for terrain deformation.
        /// </summary>
        public PriorityList<BattleEvent> OnHitTiles;

        /// <summary>
        /// Occurs after all targets are hit by the skill.
        /// In a skill with multiple strikes, this event will be called at the end of each strike.
        /// </summary>
        public PriorityList<BattleEvent> AfterActions;

        /// <summary>
        /// Modifies the elemental effect system.
        /// </summary>
        public PriorityList<ElementEffectEvent> ElementEffects;

        /// <summary>
        /// VFX that play target before they are hit.
        /// Will always play, even if the evasion roll results in a miss.
        /// </summary>
        public List<BattleFX> IntroFX;

        /// <summary>
        /// VFX that play when the target is hit.
        /// Only plays if the target is actually hit.
        /// </summary>
        public BattleFX HitFX;

        /// <summary>
        /// Target character animation when it is hit.
        /// Only plays if the target is actually hit.
        /// </summary>
        public CharAnimData HitCharAction;

        public BattleData()
        {
            ID = "";

            HitRate = -1;
            Element = "";

            SkillStates = new StateCollection<SkillState>();

            BeforeTryActions = new PriorityList<BattleEvent>();
            BeforeActions = new PriorityList<BattleEvent>();
            OnActions = new PriorityList<BattleEvent>();
            BeforeExplosions = new PriorityList<BattleEvent>();
            BeforeHits = new PriorityList<BattleEvent>();
            OnHits = new PriorityList<BattleEvent>();
            OnHitTiles = new PriorityList<BattleEvent>();
            AfterActions = new PriorityList<BattleEvent>();
            ElementEffects = new PriorityList<ElementEffectEvent>();

            IntroFX = new List<BattleFX>();
            HitFX = new BattleFX();
            HitCharAction = new CharAnimProcess();
        }

        public BattleData(BattleData other)
        {
            ID = other.ID;
            DataType = other.DataType;

            Element = other.Element;
            Category = other.Category;

            HitRate = other.HitRate;

            SkillStates = other.SkillStates.Clone();

            copyEventList<BattleEvent>(ref BeforeTryActions, other.BeforeTryActions, true);
            copyEventList<BattleEvent>(ref BeforeActions, other.BeforeActions, true);
            copyEventList<BattleEvent>(ref OnActions, other.OnActions, true);
            copyEventList<BattleEvent>(ref BeforeExplosions, other.BeforeExplosions, true);
            copyEventList<BattleEvent>(ref BeforeHits, other.BeforeHits, true);
            copyEventList<BattleEvent>(ref OnHits, other.OnHits, true);
            copyEventList<BattleEvent>(ref OnHitTiles, other.OnHitTiles, true);
            copyEventList<BattleEvent>(ref AfterActions, other.AfterActions, true);
            copyEventList<ElementEffectEvent>(ref ElementEffects, other.ElementEffects, true);

            IntroFX = new List<BattleFX>();
            foreach (BattleFX fx in other.IntroFX)
                IntroFX.Add(new BattleFX(fx));
            HitFX = new BattleFX(other.HitFX);
            HitCharAction = other.HitCharAction.Clone();
        }

        protected void copyEventList<T>(ref PriorityList<T> effectsTo, PriorityList<T> effectsFrom, bool init) where T : GameEvent
        {
            if (init)
                effectsTo = new PriorityList<T>();
            foreach (Priority priority in effectsFrom.GetPriorities())
            {
                foreach(T effect in effectsFrom.GetItems(priority))
                    effectsTo.Add(priority, (T)effect.Clone());
            }
        }

        public IEnumerator<YieldInstruction> Hit(BattleContext context)
        {
            DungeonScene.EventEnqueueFunction<BattleEvent> function = (StablePriorityQueue<GameEventPriority, EventQueueElement<BattleEvent>> queue, Priority maxPriority, ref Priority nextPriority) =>
            {
                //include the universal effect here
                DataManager.Instance.UniversalEvent.AddEventsToQueue(queue, maxPriority, ref nextPriority, DataManager.Instance.UniversalEvent.OnHits, null);
                ZoneManager.Instance.CurrentMap.MapEffect.AddEventsToQueue(queue, maxPriority, ref nextPriority, ZoneManager.Instance.CurrentMap.MapEffect.OnHits, null);
                AddEventsToQueue<BattleEvent>(queue, maxPriority, ref nextPriority, OnHits, null);
            };
            foreach (EventQueueElement<BattleEvent> effect in DungeonScene.IterateEvents<BattleEvent>(function))
                yield return CoroutineManager.Instance.StartCoroutine(effect.Event.Apply(effect.Owner, effect.OwnerChar, context));
        }
    }
}
