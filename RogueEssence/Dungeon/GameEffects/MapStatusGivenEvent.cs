using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class MapStatusGivenEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character, MapStatus status, bool msg);
    }

    [Serializable]
    public class MapStatusScriptEvent : MapStatusGivenEvent
    {
        public string Script;

        public MapStatusScriptEvent() { Script = ""; }
        protected MapStatusScriptEvent(MapStatusScriptEvent other)
        {
            Script = other.Script;
        }
        public override GameEvent Clone() { return new MapStatusScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character, MapStatus status, bool msg)
        {
            object[] parameters = new object[] { owner, ownerChar, character, status, msg };
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(Script, parameters);

            return ScriptEvent.ApplyFunc(func_iter);
        }
    }
}