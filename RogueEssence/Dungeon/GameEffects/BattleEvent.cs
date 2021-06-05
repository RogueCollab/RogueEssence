using NLua;
using RogueEssence.Script;
using System;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class BattleEvent : GameEvent
    {
        public abstract IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, BattleContext context);
    }

    [Serializable]
    public class BattleScriptEvent : BattleEvent
    {
        public string Script;

        public BattleScriptEvent() { Script = ""; }
        protected BattleScriptEvent(BattleScriptEvent other)
        {
            Script = other.Script;
        }
        public override GameEvent Clone() { return new BattleScriptEvent(this); }

        public override IEnumerator<YieldInstruction> Apply(GameEventOwner owner, Character ownerChar, BattleContext context)
        {
            object[] parameters = new object[] { owner, ownerChar, context };
            LuaFunction func_iter = LuaEngine.Instance.CreateCoroutineIterator(Script, parameters);

            return ScriptEvent.ApplyFunc(func_iter);
        }
    }

}

