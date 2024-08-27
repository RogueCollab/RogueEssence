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
}

