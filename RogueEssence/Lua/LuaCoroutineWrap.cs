/*
 * LuaCoroutineWrap.cs
 * 2017/07/04
 * psycommando@gmail.com

 */
using NLua;
using System.Collections.Generic;
using System.Linq;

namespace RogueEssence.Script
{
    /// <summary>
    /// Wraps a lua coroutine in a C# class for executing it via the engine's regular coroutine system.
    /// </summary>
    class LuaCoroutineWrap
    {
        private static readonly LuaFunction s_CoResume = LuaEngine.Instance.RunString("return coroutine.resume").First() as LuaFunction;
        private static readonly LuaFunction s_CoCreate = LuaEngine.Instance.RunString("return coroutine.create").First() as LuaFunction;
        private static readonly LuaFunction s_CoStatus = LuaEngine.Instance.RunString("return coroutine.status").First() as LuaFunction;
        private object m_Co;
        public List<object> CoReturns { get; set; } //Returned values from the Coroutine
        public object       Current { get { return CoReturns.Last(); } internal set { CoReturns.Add(value); } }


        public enum EStatus
        {
            Dead,
            Running,
            Suspended,
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fn"></param>
        public LuaCoroutineWrap(LuaFunction fn)
        {
            m_Co = s_CoCreate.Call(fn).First() as object;
        }

        public LuaCoroutineWrap(string fnpath)
        {
            m_Co = s_CoCreate.Call(LuaEngine.Instance.LuaState.GetFunction(fnpath)).First() as object;
        }

        /// <summary>
        /// Resume the coroutine.
        /// </summary>
        /// <param name="arguments">Pass those arguments to resume the coroutine</param>
        /// <returns>Return value of the coroutine, or null.</returns>
        public object Resume(params object[] arguments)
        {
            object[] res = s_CoResume.Call(m_Co,arguments);
            if ((bool)res[0])
                return Current = res[1];
            else
                return Current = null;
        }

        /// <summary>
        /// Return the state of the lua coroutine.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns>Status code for the coroutine.</returns>
        public EStatus Status()
        {
            string res = s_CoStatus.Call(m_Co).First() as string;

            if (res == "suspended")
                return EStatus.Suspended;
            else if (res == "running")
                return EStatus.Running;
            return EStatus.Dead;
        }

        /// <summary>
        /// Whether the coroutine is done or not.
        /// </summary>
        /// <returns></returns>
        public bool IsDead()
        {
            return Status() == EStatus.Dead;
        }

        /// <summary>
        /// Allows iterating over the coroutine's yield statements.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public IEnumerator<object> Iterate(params object[] arguments)
        {
            while (!IsDead())
                yield return Current = Resume(arguments);
        }
    }

}
