using NLua;
using RogueEssence.Ground;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Script
{
    /// <summary>
    /// Class grouping Task related functions exposed to the script engine.
    /// </summary>
    class ScriptTask : ILuaEngineComponent
    {

        //===========================================
        //  Methods
        //===========================================


        /// <summary>
        /// Helper function to make an entity run the specified task.
        /// Will not replace a running task!
        /// Tasks are run interlocked with the script processing and game processing, and characters cannot run multiple tasks at the same time.
        /// </summary>
        /// <param name="ent">Entity which will run the task.</param>
        /// <param name="fn">Task coroutine.</param>
        public GroundScriptedTask StartEntityTask(GroundEntity ent, LuaFunction fn)
        {
            try
            {
                if (ent == null || fn == null)
                {
                    throw new ArgumentNullException("ScriptTask.StartEntityTask(): Got null entity or function pointer!");
                }
                if (ent.GetType().IsSubclassOf(typeof(BaseTaskUser)))
                {
                    BaseTaskUser tu = (BaseTaskUser)ent;
                    GroundScriptedTask task = new GroundScriptedTask(fn);
                    if (tu.SetTask(task))
                        return task;
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return null;
        }

        /// <summary>
        /// Helper function to force stop an entity's current task.
        /// </summary>
        /// <param name="ent">Entity running the task to stop.</param>
        public void StopEntityTask(GroundEntity ent)
        {
            try
            {
                if (ent.GetType().IsSubclassOf(typeof(BaseTaskUser)))
                {
                    BaseTaskUser tu = (BaseTaskUser)ent;
                    GroundTask task = tu.CurrentTask();
                    if (task != null)
                        task.ForceStop();
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
        }

        /// <summary>
        /// Makes an entity run a specified task, and waits for it to complete.
        /// </summary>
        /// <param name="ent">Entity which will run the task.</param>
        /// <param name="fn">Task coroutine.</param>
        /// <example>
        /// TODO
        /// </example>
        public LuaFunction WaitStartEntityTask;

        public Coroutine _WaitStartEntityTask(GroundEntity ent, LuaFunction fn)
        {
            try
            {
                if (ent == null || fn == null)
                    throw new ArgumentNullException("ScriptTask._WaitStartEntityTask(): Got null entity or function pointer!");
                if (ent.GetType().IsSubclassOf(typeof(BaseTaskUser)))
                {
                    BaseTaskUser tu = (BaseTaskUser)ent;
                    return new Coroutine(tu.WaitSetTask(new GroundScriptedTask(fn)));
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return new Coroutine(LuaEngine._DummyWait());
        }

        /// <summary>
        /// Waits for the specified entity to finish its task.
        /// </summary>
        /// <param name="ent">Entity which task we'll wait on.</param>
        /// <example>
        /// TASK:WaitEntityTask(player)
        /// </example>
        public LuaFunction WaitEntityTask;

        public Coroutine _WaitEntityTask(GroundEntity ent)
        {
            try
            {
                if (ent.GetType().IsSubclassOf(typeof(BaseTaskUser)))
                {
                    BaseTaskUser tu = (BaseTaskUser)ent;
                    if (tu.CurrentTask() != null)
                        return tu.CurrentTask().Wait();
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
            }
            return new Coroutine(LuaEngine._DummyWait());
        }


        /// <summary>
        /// Runs a task and waits for it to complete.
        /// Most methods that do not expose themselves to script need to be wrapped with this.
        /// </summary>
        /// <param name="obj">The task to wait on.</param>
        /// <example>
        /// TASK:WaitTask(_DUNGEON:DropMoney(100, RogueElements.Loc(10, 10), RogueElements.Loc(10, 10)))
        /// </example>
        public LuaFunction WaitTask;
        public Coroutine _WaitTask(object obj)
        {
            if (obj is IEnumerator<YieldInstruction>)
            {
                Coroutine coro = new Coroutine(obj as IEnumerator<YieldInstruction>);
                return coro;
            }
            else if (obj is Coroutine)
            {
                Coroutine coro = obj as Coroutine;
                return coro;
            }
            return null;
        }

        //===================================
        // Coroutines
        //===================================

        /// <summary>
        /// A wrapper around the StartCoroutine method of the GameManager, so lua coroutines can be executed locally to the script context.
        /// AKA, it will block the script execution while its executed.
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Coroutine StartScriptLocalCoroutine(LuaFunction fn, params object[] args)
        {
            try
            {
                return CoroutineManager.Instance.StartCoroutine(new LuaCoroutineIterator(fn, args));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex, DiagManager.Instance.DevMode);
                return null;
            }
        }

        /// <summary>
        /// Starts a new coroutine to run parallel to the current execution.
        /// Useful for performing multiple actions at once.
        /// </summary>
        /// <param name="obj">The task to run in parallel</param>
        /// <example>
        /// local coro1 = TASK:BranchCoroutine(GAME:_FadeIn(60))
        /// </example>
        public Coroutine BranchCoroutine(object obj)
        {
            if (obj is Coroutine)
            {
                Coroutine coro = obj as Coroutine;
                return CoroutineManager.Instance.StartCoroutine(coro, true);
            }
            else if (obj is LuaFunction)
            {
                LuaFunction luaFun = obj as LuaFunction;
                return CoroutineManager.Instance.StartCoroutine(new Coroutine(LuaEngine.Instance.CallScriptFunction(luaFun)), true);
            }
            return null;
        }

        /// <summary>
        /// Waits for all specified coroutines to finish before continuing execution.
        /// Often used for coroutines created using TASK:BranchCoroutine()
        /// </summary>
        /// <param name="coroTable">A table of coroutines to wait on.</param>
        /// <example>
        /// TASK:JoinCoroutines({coro1})
        /// </example>
        public LuaFunction JoinCoroutines;

        public Coroutine _JoinCoroutines(LuaTable coroTable)
        {
            List<Coroutine> coroutines = new List<Coroutine>();
            foreach (object val in coroTable.Values)
                coroutines.Add((Coroutine)val);
            return new Coroutine(_WaitForTasksDone(coroutines));
        }

        private IEnumerator<YieldInstruction> _WaitForTasksDone(List<Coroutine> coroutines)
        {
            while (true)
            {
                for (int ii = coroutines.Count - 1; ii >= 0; ii--)
                {
                    if (coroutines[ii].FinishedYield())
                        coroutines.RemoveAt(ii);
                }
                if (coroutines.Count == 0)
                    yield break;
                yield return new WaitForFrames(1);
            }
        }

        //===========================================
        //  Setup pure lua functions
        //===========================================
        public override void SetupLuaFunctions(LuaEngine state)
        {
            WaitStartEntityTask = state.RunString("return function(_, ent, fun) return coroutine.yield(TASK:_WaitStartEntityTask(ent, fun)) end").First() as LuaFunction;
            WaitEntityTask = state.RunString("return function(_, ent) return coroutine.yield(TASK:_WaitEntityTask(ent)) end").First() as LuaFunction;
            JoinCoroutines = state.RunString("return function(_, ent) return coroutine.yield(TASK:_JoinCoroutines(ent)) end").First() as LuaFunction;
            WaitTask = state.RunString("return function(_, ent) return coroutine.yield(TASK:_WaitTask(ent)) end").First() as LuaFunction;
        }
    }
}
