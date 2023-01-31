using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class ItemGivenEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, ItemCheckContext context);
    }

    [Serializable]
    public class ItemScriptEvent : ItemGivenEvent
    {
        [Dev.Sanitize(0)]
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

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, ItemCheckContext context)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ArgTable).First() as LuaTable;
            object[] parameters = new object[] { owner, ownerChar, context, args };
            string name = "ITEM_SCRIPT." + Script;
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(name, parameters);

            yield return CoroutineManager.Instance.StartCoroutine(ScriptEvent.ApplyFunc(name, func_iter));
        }
    }
}
