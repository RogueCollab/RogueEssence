using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Linq;

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
        [Dev.Multiline(0)]
        public string ArgTable;

        public MapStatusScriptEvent() { Script = ""; ArgTable = "{}"; }
        public MapStatusScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public MapStatusScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected MapStatusScriptEvent(MapStatusScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new MapStatusScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, Character character, MapStatus status, bool msg)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, character, status, msg, args };
            string name = "MAP_STATUS_SCRIPT." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }
    }
}