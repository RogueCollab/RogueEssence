using System;
using System.Collections.Generic;
using System.Linq;
using NLua;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Ground
{
    public enum SelectionType
    {
        /// <summary>
        /// Apply to user
        /// </summary>
        Self,
        /// <summary>
        /// Apply to other party members
        /// </summary>
        Others,
    }
    
    public abstract class GroundItemEvent : GameEvent
    {
        /// <summary>
        /// The ground item menu label
        /// </summary>
        public ItemData.UseType GroundUsageType;
        
        /// <summary>
        /// Determines who the item should target
        /// </summary>
        public SelectionType Selection;
        public abstract IEnumerator<YieldInstruction> Apply(GroundContext context);
        
        /// <summary>
        /// Whether the action is hidden in the item menu
        /// </summary>
        public bool Hidden;
        
        /// <summary>
        /// Whether the action is disabled in the item menu
        /// </summary>
        public bool Disabled;
    }
    
    [Serializable]
    public class ScriptItemEvent : GroundItemEvent
    {
        [Dev.Sanitize(0)]
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public ScriptItemEvent() { Script = ""; ArgTable = "{}"; }
        public ScriptItemEvent(string script) { Script = script; ArgTable = "{}"; }
        public ScriptItemEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected ScriptItemEvent(ScriptItemEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new ScriptItemEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GroundContext context)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { context, args };
            string name = "GROUND_ITEM_EVENT_SCRIPT." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }
    }
    
    [Serializable]
    public class LearnItemEvent : GroundItemEvent
    {
        [DataType(0, DataManager.DataType.Skill, false)] 
        public string Skill;
        public LearnItemEvent() { Skill = ""; }
        public LearnItemEvent(string skill) { Skill = skill; }
        protected LearnItemEvent(LearnItemEvent other) { Skill = other.Skill; }

        public override GameEvent Clone() { return new LearnItemEvent(this); }
        public override IEnumerator<YieldInstruction> Apply(GroundContext context)
        {
            Character target = context.User;
            
            int learn = -1;
            
            yield return CoroutineManager.Instance.StartCoroutine(DungeonScene.TryLearnSkill(target, Skill, (int slot) => { learn = slot; }, () => { context.CancelState.Cancel = true; }));
            if (context.CancelState.Cancel) yield break;
            
            yield return CoroutineManager.Instance.StartCoroutine(
                DungeonScene.LearnSkillWithFanfare(target, Skill, learn));
        }
    }
}