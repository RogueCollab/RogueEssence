using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class BattleEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, BattleContext context);
    }

    [Serializable]
    public class BattleScriptEvent : BattleEvent
    {
        [Dev.Sanitize(0)]
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public BattleScriptEvent() { Script = ""; ArgTable = "{}"; }
        public BattleScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public BattleScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected BattleScriptEvent(BattleScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new BattleScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, BattleContext context)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, context, args };
            string name = LuaEngine.EVENT_BATTLE_NAME + "." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }
    }


    [Serializable]
    public class BattleScriptStateEvent : BattleEvent
    {
        public BattleScriptStateEvent() {  }
        public override GameEvent Clone() { return new BattleScriptStateEvent(); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, BattleContext context)
        {
            ScriptCallState call = ((StatusEffect)owner).StatusStates.Get<ScriptCallState>();
            LuaTable args = LuaEngine.Instance.RunString("return " + call.ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, context, args };
            string name = LuaEngine.EVENT_BATTLE_NAME + "." + call.Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }
    }

    /// <summary>
    /// Event that activates if the supplied condition script returns true
    /// If the script does not return a boolean, it will still count as true as long as it is not nil
    /// </summary>
    public class ScriptedConditionEvent : BattleScriptEvent
    {
        /// <summary>
        /// The list of battle events that plays if the condition is met
        /// </summary>
        public List<BattleEvent> BaseEvents;

        public ScriptedConditionEvent() { Script = ""; ArgTable = "{}"; BaseEvents = new List<BattleEvent>(); }
        public ScriptedConditionEvent(string script) { Script = script; ArgTable = "{}"; BaseEvents = new List<BattleEvent>(); }
        public ScriptedConditionEvent(string script, string argTable, List<BattleEvent> events) { Script = script; ArgTable = argTable; BaseEvents = events; }
        protected ScriptedConditionEvent(ScriptedConditionEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
            BaseEvents = other.BaseEvents;
        }
        public override GameEvent Clone() { return new ScriptedConditionEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, BattleContext context)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, context, args };
            string name = LuaEngine.EVENT_CONDITION_NAME + "." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            object result = func_iter.Call().First();
            if (result is bool && (bool)result == true || result is not null)
            {
                foreach (BattleEvent battleEffect in BaseEvents)
                    yield return CoroutineManager.Instance.StartCoroutine(battleEffect.Apply(owner, ownerChar, context));
            }

        }
    }
}

