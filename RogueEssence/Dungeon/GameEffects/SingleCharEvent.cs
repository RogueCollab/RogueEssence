using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;

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

        public SingleCharScriptEvent() { Script = ""; }
        protected SingleCharScriptEvent(SingleCharScriptEvent other)
        {
            Script = other.Script;
        }
        public override GameEvent Clone() { return new SingleCharScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character)
        {
            object[] parameters = new object[] { owner, ownerChar, character };
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(Script, parameters);

            return ScriptEvent.ApplyFunc(func_iter);
        }
    }
}
