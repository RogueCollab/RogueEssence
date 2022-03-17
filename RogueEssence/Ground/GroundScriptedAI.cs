using System;
using System.Collections.Generic;
using System.Linq;
using RogueEssence.Script;
using NLua;
using System.Runtime.Serialization;

namespace RogueEssence.Ground
{
    /// <summary>
    /// Implementation of the GroundAI that works using AI templates defined inside lua scripts.
    /// </summary>
    [Serializable]
    class GroundScriptedAI : GroundAI
    {
        private static readonly string FieldCurrentStateName = "CurrentState";
        private static readonly string FieldNextStateName = "NextState";
        private static readonly string FieldLastStateName = "LastState";
        //private static readonly string FieldStatesName = "States";


        //Lua function names are stored here
        private static readonly string FunUpdateName = "Update";

        ///For this property, we want to wrap  it around accessing the luatable of the current AI template instance!
        public override string CurrentState
        {
            get
            {
                if(m_AITemplate != null)
                {
                    string state = m_AITemplate[FieldCurrentStateName] as string;
                    if (!String.IsNullOrEmpty(state))
                        return state;
                }
                return null;
            }
            protected set
            {
                if (m_AITemplate != null)
                    m_AITemplate[FieldCurrentStateName] = value;
                else
                    DiagManager.Instance.LogInfo(String.Format("GroundScriptedAI.CurrentState.set: Tried to set the current state to \"{0}\" before the AI template was loaded!", value));
            }
        }

        /// <summary>
        /// Look at the AI instance class to get the last state
        /// </summary>
        public string LastState
        {
            get
            {
                if (m_AITemplate != null)
                {
                    string state = m_AITemplate[FieldLastStateName] as string;
                    if (!String.IsNullOrEmpty(state))
                        return state;
                }
                return null;
            }
        }

        /// <summary>
        /// Look at the AI template instance to get the next state
        /// </summary>
        public string NextState
        {
            get
            {
                if (m_AITemplate != null)
                {
                    string state = m_AITemplate[FieldNextStateName] as string;
                    if (!String.IsNullOrEmpty(state))
                        return state;
                }
                return null;
            }
        }


        /// <summary>
        /// An instance of the lua AI Template currently being used.
        /// </summary>
        [NonSerialized] private LuaTable m_AITemplate = null;

        /// <summary>
        /// Keeps a reference on the AI iterator.
        /// </summary>
        [NonSerialized] private Coroutine m_AICoro = null;

        /// <summary>
        /// Holds the current classpath to the AI to run, so when de-serializing we can reload the proper AI.
        /// </summary>
        private string m_AIClasspath;

        /// <summary>
        /// Holds the arguments issued to the AI constructor
        /// </summary>
        private object[] m_Arguments;

        /// <summary>
        /// The AI's update function is cached+compiled in here.
        /// </summary>
        [NonSerialized] private LuaFunction m_fnUpdate;

        /// <summary>
        /// Constructor for the ground scripted AI which takes a path to the lua class to instantiate and use as AI Template.
        /// </summary>
        /// <param name="luaAIclasspath"></param>
        public GroundScriptedAI(string luaAIclasspath, params object[] args)
        {
            m_AIClasspath = luaAIclasspath;
            m_Arguments = args;
            InstantiateAI();
        }

        /// <summary>
        /// Instanciate the AI module we're set to use for this entity, and keep the instance in m_AITemplate.
        /// </summary>
        protected void InstantiateAI()
        {
            if (!String.IsNullOrEmpty(m_AIClasspath))
            {
                m_AITemplate = LuaEngine.Instance.InstantiateLuaModule(m_AIClasspath, m_Arguments);
                m_fnUpdate = GetAIFunction(FunUpdateName);
            }
            else
            {
                m_AITemplate = null;
                m_fnUpdate = null;
            }
        }

        /// <summary>
        /// Copy the AI's given function, and return it.
        /// </summary>
        /// <param name="fname"></param>
        /// <returns></returns>
        private LuaFunction GetAIFunction(string fname)
        {
            try
            {
                //LuaFunction getfn = LuaEngine.Instance.RunString("return function(ai, funname) return function(...) return ai[funname](ai,...); end end").First() as LuaFunction;
                LuaFunction getfn = LuaEngine.Instance.RunString("return function(ai, funname) return ai[funname] end").First() as LuaFunction;
                return getfn.Call(m_AITemplate, fname).First() as LuaFunction;
            }
            catch( Exception ex )
            {
                DiagManager.Instance.LogInfo(String.Format("GroundScriptedAI.GetAIFunction(): Couldn't find a matching lua function at {0}.{1}! AI disabled!\nDetails:\n{2}", m_AIClasspath, fname, ex.Message));
                return null;
            }
        }

        /// <summary>
        /// Runs one of the functions
        /// </summary>
        /// <param name="funname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private IEnumerator<YieldInstruction> RunAIUpdateFun()
        {
            if (m_fnUpdate == null)
                yield break;

            LuaCoroutineIterator coro = new LuaCoroutineIterator(m_fnUpdate, m_AITemplate, EntityPointer);
            while (coro.MoveNext())
                yield return coro.Current;
        }

        public override void ForceState(string statename)
        {
            CurrentState = statename;
        }


        public override void UpdateAI()
        {
            if (m_AITemplate == null)
            {
                DiagManager.Instance.LogInfo("GroundScriptedAI.UpdateAI(): No AI template assigned to this instance!!");
                return;
            }

            if (m_AICoro == null)
                m_AICoro = CoroutineManager.Instance.StartCoroutine(RunAIUpdateFun());

            if (m_AICoro.FinishedYield())
                m_AICoro = null;
        }


        public override void OnMapInit()
        {
            InstantiateAI();
        }
    }
}
