using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class StatusGivenEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, StatusCheckContext context);
    }

    [Serializable]
    public class StatusScriptEvent : StatusGivenEvent
    {
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public StatusScriptEvent() { Script = ""; ArgTable = "{}"; }
        public StatusScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public StatusScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected StatusScriptEvent(StatusScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new StatusScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, StatusCheckContext context)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, context, args };
            string name = "STATUS_SCRIPT." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            return ScriptEvent.ApplyFunc(name, func_iter);
        }
    }
}
