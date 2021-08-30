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
        // Pure lua functions
        //===========================================
        public LuaFunction WaitStartEntityTask;
        public LuaFunction WaitEntityTask;
        public LuaFunction WaitTask;
        public LuaFunction JoinCoroutines;

        //===========================================
        //  Methods
        //===========================================
        /// <summary>
        /// Helper function to make an entity run the specified task.
        /// Will not replace a running task!
        /// Tasks are run interlocked with the script processing and game processing, and characters can run tasks at the same time.
        /// </summary>
        /// <param name="ent">Entity which will run the task.</param>
        /// <param name="fn">Task coroutine.</param>
        /// <returns>Returns whether the task could be set or not</returns>
        public GroundScriptedTask StartEntityTask(GroundEntity ent, LuaFunction fn)
        {
            try
            {
                if (ent == null || fn == null)
                {
                    DiagManager.Instance.LogInfo(String.Format("ScriptTask.StartEntityTask(): Got null entity or function pointer!"));
                    return null;
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
                DiagManager.Instance.LogInfo(String.Format("ScriptTask.StartEntityTask(): Got exception :\n{0}", ex.Message));
            }
            return null;
        }

        /// <summary>
        /// Same as StartEntityTask, but this one blocks until the task is set
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="fn"></param>
        public Coroutine _WaitStartEntityTask(GroundEntity ent, LuaFunction fn)
        {
            try
            {
                if (ent == null || fn == null)
                    DiagManager.Instance.LogInfo(String.Format("ScriptTask._WaitStartEntityTask(): Got null entity or function pointer!"));
                if (ent.GetType().IsSubclassOf(typeof(BaseTaskUser)))
                {
                    BaseTaskUser tu = (BaseTaskUser)ent;
                    return new Coroutine(tu.WaitSetTask(new GroundScriptedTask(fn)));
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptTask._WaitStartEntityTask(): Got exception :\n{0}", ex.Message));
            }
            return new Coroutine(LuaEngine._DummyWait());
        }

        /// <summary>
        /// Helper function to force stop an entity's current task!
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
                DiagManager.Instance.LogInfo(String.Format("ScriptTask.StopEntityTask(): Got exception :\n{0}", ex.Message));
            }
        }

        /// <summary>
        /// Helper function to invoke the Wait() function of the current task of the specified entity.
        /// </summary>
        /// <param name="ent">Entity which task we'll wait on.</param>
        /// <returns></returns>
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
                DiagManager.Instance.LogInfo(String.Format("ScriptTask._WaitEntityTask(): Got exception :\n{0}", ex.Message));
            }
            return new Coroutine(LuaEngine._DummyWait());
        }


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
        /// <param name = "fn" ></ param >
        /// < param name= "args" ></ param >
        /// < returns ></ returns >
        public Coroutine StartScriptLocalCoroutine(LuaFunction fn, params object[] args)
        {
            try
            {
                return CoroutineManager.Instance.StartCoroutine(new LuaCoroutineIterator(fn, args));
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogInfo(String.Format("ScriptTask.StartScriptLocalCoroutine(): Got exception :\n{0}", ex.Message));
                return null;
            }
        }

        public object GetArrayValue(object arr, int index)
        {
            if (arr is Array)
            {
                Array array = arr as Array;
                return array.GetValue(index);
            }
            return null;
        }

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
                return CoroutineManager.Instance.StartCoroutine(new Coroutine(callScriptFunction(luaFun)), true);
            }
            return null;
        }

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

        private IEnumerator<YieldInstruction> callScriptFunction(LuaFunction luaFun)
        {
            //Create a lua iterator function for the lua coroutine
            LuaFunction iter = LuaEngine.Instance.CreateCoroutineIterator(luaFun);

            //Then call it until it returns null!
            object[] allres = iter.Call();
            object res = allres.First();
            while (res != null)
            {
                if (res.GetType() == typeof(Coroutine)) //This handles waiting on coroutines
                    yield return CoroutineManager.Instance.StartCoroutine(res as Coroutine, false);
                else if (res.GetType().IsSubclassOf(typeof(YieldInstruction)))
                    yield return res as YieldInstruction;

                //Pick another yield from the lua coroutine
                allres = iter.Call();
                res = allres.First();
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
