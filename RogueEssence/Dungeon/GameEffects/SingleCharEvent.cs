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
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character);
    }

    [Serializable]
    public class SingleCharScriptEvent : SingleCharEvent
    {
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

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, character, args };
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator("SINGLE_CHAR_SCRIPT." + Script, parameters);

            return ScriptEvent.ApplyFunc(func_iter);
        }
    }
}
