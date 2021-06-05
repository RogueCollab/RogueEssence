using NLua;
using RogueEssence.Script;
using System;

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

        public RefreshScriptEvent() { Script = ""; }
        protected RefreshScriptEvent(RefreshScriptEvent other)
        {
            Script = other.Script;
        }
        public override GameEvent Clone() { return new RefreshScriptEvent(this); }

        public override void Apply(GameEventOwner owner, Character ownerChar, Character character)
        {
            LuaFunction luafun = LuaEngine.Instance.LuaState.GetFunction(Script);

            if (luafun != null)
                luafun.Call(new object[] { owner, ownerChar, character });
        }
    }
}
