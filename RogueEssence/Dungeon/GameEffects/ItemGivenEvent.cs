using NLua;
using RogueEssence.Script;
using System;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class ItemGivenEvent : GameEvent
    {
        public abstract void Apply(GameEventOwner owner, Character ownerChar, ItemCheckContext context);
    }

    [Serializable]
    public class ItemScriptEvent : ItemGivenEvent
    {
        public string Script;
        [Dev.Multiline(0)]
        public string ArgTable;

        public ItemScriptEvent() { Script = ""; ArgTable = "{}"; }
        public ItemScriptEvent(string script) { Script = script; ArgTable = "{}"; }
        public ItemScriptEvent(string script, string argTable) { Script = script; ArgTable = argTable; }
        protected ItemScriptEvent(ItemScriptEvent other)
        {
            Script = other.Script;
            ArgTable = other.ArgTable;
        }
        public override GameEvent Clone() { return new ItemScriptEvent(this); }

        public override void Apply(GameEventOwner owner, Character ownerChar, ItemCheckContext context)
        {
            LuaFunction luafun = LuaEngine.Instance.LuaState.GetFunction("ITEM_SCRIPT." + Script);

            if (luafun != null)
            {
                LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
                luafun.Call(new object[] { owner, ownerChar, context, args });
            }
        }
    }
}
