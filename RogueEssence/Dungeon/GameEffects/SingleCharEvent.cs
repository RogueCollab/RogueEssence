using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Dungeon
{

    [Serializable]
    public abstract class SingleCharEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, SingleCharContext context);
    }

    [Serializable]
    public class SingleCharScriptEvent : SingleCharEvent
    {
        [Dev.Sanitize(0)]
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public SingleCharScriptEvent() { Script = ""; ArgTable = "{}"; }
        public SingleCharScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public SingleCharScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected SingleCharScriptEvent(SingleCharScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new SingleCharScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, SingleCharContext context)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, context.User, args };
            string name = "SINGLE_CHAR_SCRIPT." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }
    }
}
