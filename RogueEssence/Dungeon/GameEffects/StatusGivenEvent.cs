using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class StatusGivenEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, StatusCheckContext context);
    }

    [Serializable]
    public class StatusGivenScriptEvent : StatusGivenEvent
    {
        public string Script;

        public StatusGivenScriptEvent() { Script = ""; }
        protected StatusGivenScriptEvent(StatusGivenScriptEvent other)
        {
            Script = other.Script;
        }
        public override GameEvent Clone() { return new StatusGivenScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, StatusCheckContext context)
        {
            object[] parameters = new object[] { owner, ownerChar, context };
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(Script, parameters);

            return ScriptEvent.ApplyFunc(func_iter);
        }
    }
}
