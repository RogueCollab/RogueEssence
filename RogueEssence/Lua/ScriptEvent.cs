/*
 * ScriptCoroutines.cs
 * 2017/07/01
 * psycommando@gmail.com

 */
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RogueEssence.Script
{
    /// <summary>
    /// Base class for all script events
    /// </summary>
    [Serializable]
    public abstract class BaseScriptEvent
    {
        /// <summary>
        /// Called to create a copy of this instance
        /// </summary>
        /// <returns></returns>
        public abstract BaseScriptEvent Clone();

        /// <summary>
        /// Returns a name for this event
        /// </summary>
        /// <returns></returns>
        public abstract string EventName();

        /// <summary>
        /// Runs the event with the given parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public abstract IEnumerator<YieldInstruction> Apply(params object[] parameters);

        /// <summary>
        /// Called to execute cleanup on map change.
        /// </summary>
        public virtual void DoCleanup() { }


        /// <summary>
        /// This is called to make the event relink with the lua state
        /// </summary>
        public abstract void ReloadEvent();

        /// <summary>
        /// This is called when the Lua engine reloads
        /// </summary>
        public abstract void LuaEngineReload();
    };


    /// <summary>
    /// An event which calls a script function when triggered!
    /// </summary>
    [Serializable]
    public class ScriptEvent : BaseScriptEvent
    {
        protected string m_luapath;
        [NonSerialized] protected LuaFunction   m_iterator;
        [NonSerialized] protected bool          m_bfunvalid;
        [NonSerialized] protected bool          m_bforcebreak = false;

        public override string EventName()
        {
            return m_luapath;
        }

        /// <summary>
        /// Protected blank constructor, for child classes
        /// </summary>
        protected ScriptEvent()
        {
            m_luapath = null;
            m_iterator = null;
            m_bfunvalid = false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="luafunpath"></param>
        public ScriptEvent(string luafunpath)
        {
            SetLuaFunctionPath(luafunpath);
        }

        public void SetLuaFunctionPath(string luafunpath)
        {
            m_luapath = luafunpath;
            m_bfunvalid = LuaEngine.Instance.DoesFunctionExists(m_luapath); //Make an initial check for that, and keeps the event from running
            if (!m_bfunvalid && m_luapath != null)
                DiagManager.Instance.LogInfo(String.Format("ScriptEvent(): Lua function '{0}' does not exists. The event will not run!", m_luapath));
        }

        public override BaseScriptEvent Clone()
        {
            return new ScriptEvent(m_luapath);
        }

        /// <summary>
        /// Called when the event is about to be removed from the context. Add everything that needs to be done before the event is removed in here.
        /// </summary>
        public override void DoCleanup()
        {
            DiagManager.Instance.LogInfo(String.Format("ScriptEvent.DoCleanup(): Doing cleanup on {0}!", m_luapath));
            m_iterator = null;
        }

        public override IEnumerator<YieldInstruction> Apply(params object[] parameters)
        {

            if (!m_bfunvalid)
                yield break;
            //If force break had been set before we even rand this, set it back to false
            m_bforcebreak = false;

            //Create a lua iterator function for the lua coroutine
            m_iterator = MakeIterator(parameters);

            //Then call it until it returns null!
            object[] allres = CallInternal();
            object res = allres.First();
            while (res != null)
            {
                if (res.GetType() == typeof(Coroutine)) //This handles waiting on coroutines
                    yield return StartCoroutine(res as Coroutine);
                else if (res.GetType().IsSubclassOf(typeof(YieldInstruction)))
                    yield return res as YieldInstruction;

                //In case we get the order to break out of this, do it! This will happen when the engine is reloaded, and we don't want
                // to be running a coroutine when this happens!
                if (m_bforcebreak)
                {
                    DiagManager.Instance.LogInfo(String.Format("ScriptEvent.Apply(): Coroutine for event {0} interrupted by engine reload!", m_luapath));
                    m_bforcebreak = false;
                    break;
                }

                //Pick another yield from the lua coroutine
                allres = CallInternal();
                res = allres.First();
            }
        }

        /// <summary>
        /// This method handles the creation of the lua iterator. Meant to be reimplemented while subclassing the scriptevent class,
        /// changing this to fit a new purpose without having to rewrite the entire Apply method!
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual LuaFunction MakeIterator(params object[] parameters)
        {
            return LuaEngine.Instance.CreateCoroutineIterator(m_luapath, parameters);
        }

        /// <summary>
        /// Wraps the starting of sub-coroutine, so it can be inherited and changed as neeeded!
        /// </summary>
        /// <param name="coro"></param>
        /// <returns></returns>
        protected virtual Coroutine StartCoroutine(Coroutine coro)
        {
            return CoroutineManager.Instance.StartCoroutine(coro, false);
        }

        /// <summary>
        /// Wrapper around the lua iterator to catch and print any possible script errors.
        /// </summary>
        /// <returns></returns>
        protected object[] CallInternal()
        {
            try
            {
                return m_iterator.Call();
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogInfo(String.Format("[SE]:ScriptEvent.CallInternal(): Error calling coroutine iterator :\n{0}", e.Message));
            }
            return new object[] { null }; //Stop the coroutine since we errored
        }

        /// <summary>
        /// Looks into the current lua state for the function corresponding to this event's stored luapath, and determines if it can
        /// be run or not.
        /// This should be called only after the corresponding map script has been loaded, otherwise it won't find its matching function.
        /// </summary>
        public override void ReloadEvent()
        {
            DiagManager.Instance.LogInfo(String.Format("ScriptEvent.ReloadEvent(): Reloading event {0}!", m_luapath));
            m_bfunvalid = LuaEngine.Instance.DoesFunctionExists(m_luapath); //Make an initial check for that, and keeps the event from running
            if (!m_bfunvalid)
                DiagManager.Instance.LogInfo(String.Format("ScriptEvent.ReloadEvent(): Lua function '{0}' does not exists. The event will not run!", m_luapath));
        }

        //Called when the engine is reloaded.
        // Since all lua references are invalidated, we have to set them up again!
        public override void LuaEngineReload()
        {
            DiagManager.Instance.LogInfo(String.Format("ScriptEvent.OnLuaEngineReload(): Reloading event {0}, and interrupting current coroutine!", m_luapath));
            ReloadEvent();
            m_bforcebreak = true;
        }

        /// <summary>
        /// Called when events are loaded from save, or a saved map.
        /// This is called before the GroundMap's own OnDeserializedMethod.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //DiagManager.Instance.LogInfo(String.Format("ScriptEvent.OnDeserializedMethod(): Event {0} deserialized!", m_luapath));
            //Setup our delegates with the LuaEngine
        }

    }


    /// <summary>
    /// TransientScriptEvent is a script event that's not meant to be serialized,
    /// and runs for a short period of time.
    /// As a result, it can take directly a lua function as construction parameter!
    /// </summary>
    public class TransientScriptEvent : ScriptEvent
    {
        [NonSerialized] private LuaFunction m_luafun = null;

        public TransientScriptEvent(LuaFunction luafun)
        {
            m_luafun = luafun;
            m_bfunvalid = luafun != null;
            if (!m_bfunvalid)
                DiagManager.Instance.LogInfo("TransientScriptEvent.TransientScriptEvent(): lua function passed as parameter is null!");
        }
        public TransientScriptEvent(string luafun)
        {
            m_luafun = LuaEngine.Instance.RunString("return " + luafun).First() as LuaFunction;
            m_bfunvalid = luafun != null;
            if (!m_bfunvalid)
                DiagManager.Instance.LogInfo("TransientScriptEvent.TransientScriptEvent(): lua function path passed as parameter is invalid!");
        }

        public override string EventName()
        {
            return m_luafun.ToString();
        }

        protected override LuaFunction MakeIterator(params object[] parameters)
        {
            if (m_luafun == null)
                throw new Exception("TransientScriptEvent.MakeIterator(): Function is null! Make sure the transientevent isn't being deserialized and run!");
            return LuaEngine.Instance.CreateCoroutineIterator(m_luafun, parameters);
        }
    }

}
