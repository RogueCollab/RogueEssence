using NLua;
using RogueEssence.Script;
using System;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class RefreshEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character ownerChar, Character character);
    }

    [Serializable]
    public class RefreshScriptEvent : RefreshEvent
    {
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public RefreshScriptEvent() { Script = ""; ArgTable = "{}"; }
        public RefreshScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public RefreshScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected RefreshScriptEvent(RefreshScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new RefreshScriptEvent(this); }

        public override void Apply(GameEventOwner owner, Character ownerChar, Character character)
        {
            LuaFunction luafun = LuaEngine.Instance.LuaState.GetFunction("REFRESH_SCRIPT." + Script);

            if (luafun != null)
            {
                LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
                luafun.Call(new object[] { owner, ownerChar, character, args });
            }
        }
    }
}
