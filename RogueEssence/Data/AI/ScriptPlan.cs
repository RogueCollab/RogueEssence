using NLua;
using RogueEssence.Script;
using System;
using System.Linq;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public class ScriptPlan : BasePlan
    {
        [Dev.Sanitize(0)]
        public string ThinkScript;
        [Dev.Multiline(0)]
        public string ThinkArgTable;
        [Dev.Sanitize(0)]
        public string InitializeScript;
        [Dev.Multiline(0)]
        public string InitializeArgTable;
        [Dev.Sanitize(0)]
        public string SwitchedInScript;
        [Dev.Multiline(0)]
        public string SwitchedInArgTable;
        
        [NonSerialized]
        private LuaTable luaTable;

        public ScriptPlan()
        {
            luaTable = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            ThinkScript = ""; ThinkArgTable = "{}";
            InitializeScript = ""; InitializeArgTable = "{}";
            SwitchedInScript = ""; SwitchedInArgTable = "{}";
        }

        public ScriptPlan(string script) 
        {
            luaTable = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            ThinkScript = script; ThinkArgTable = "{}";
            InitializeScript = ""; InitializeArgTable = "{}";
            SwitchedInScript = ""; SwitchedInArgTable = "{}";
        }
        public ScriptPlan(string think, string thinkTable) 
        {
            luaTable = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            ThinkScript = think; ThinkArgTable = thinkTable;
            InitializeScript = ""; InitializeArgTable = "{}";
            SwitchedInScript = ""; SwitchedInArgTable = "{}";
        }
        public ScriptPlan(string think, string thinkTable, string init, string initTable, string switched, string switchedTable) 
        {
            luaTable = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
            ThinkScript = think; ThinkArgTable = thinkTable;
            InitializeScript = init; InitializeArgTable = initTable;
            SwitchedInScript = switched; SwitchedInArgTable = switchedTable;
        }
        protected ScriptPlan(ScriptPlan other)
        {
            luaTable = other.luaTable;
            ThinkScript = other.ThinkScript;
            ThinkArgTable = other.ThinkArgTable;
            InitializeScript = other.InitializeScript;
            InitializeArgTable = other.InitializeArgTable;
            SwitchedInScript = other.SwitchedInScript;
            SwitchedInArgTable = other.SwitchedInArgTable;
        }

        public override BasePlan CreateNew() { return new ScriptPlan(this); }

        public override void Initialize(Character controlledChar)
        {
            if (string.IsNullOrEmpty(InitializeScript)) return;
            LuaTable args = LuaEngine.Instance.RunString("return " + InitializeArgTable).First() as LuaTable;
            string name = LuaEngine.EVENT_AI_INIT_NAME + "." + InitializeScript;
            LuaEngine.Instance.CallLuaFunctions(name, controlledChar, luaTable, args );
        }
        
        public override void SwitchedIn(BasePlan currentPlan)
        {
            if (string.IsNullOrEmpty(SwitchedInScript)) return;
            LuaTable args = LuaEngine.Instance.RunString("return " + SwitchedInArgTable).First() as LuaTable;
            string name = LuaEngine.EVENT_AI_SWITCH_NAME + "." + SwitchedInScript;
            LuaEngine.Instance.CallLuaFunctions(name, currentPlan, luaTable, args);
        }
        
        public override GameAction Think(Character controlledChar, bool preThink, IRandom rand)
        {
            LuaTable args = LuaEngine.Instance.RunString("return " + ThinkArgTable).First() as LuaTable;
            string name = LuaEngine.EVENT_AI_THINK_NAME + "." + ThinkScript;
            object result = null;
            object[] res = LuaEngine.Instance.CallLuaFunctions(name, controlledChar, preThink, rand, luaTable, args);
            if (res is not null && res.Length > 0) result = res.First();

            if (result is GameAction action) return action;
            if (result == null) return null;
            throw new Exception("Expected return types for \"" + LuaEngine.EVENT_AI_THINK_NAME + "\" functions are: \"RogueEssence.Dungeon.GameAction\", nil. \"" + name + "\" returned a " + result.GetType().ToString());
        }

    }
}