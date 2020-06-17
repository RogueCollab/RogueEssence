using System;
using System.Collections.Generic;


namespace RogueEssence
{
    /// <summary>
    /// Runs multiple scheduled coroutines cooperatively
    /// </summary>
    public class CoroutineManager
    {
        private static Lazy<CoroutineManager> s_instance = new Lazy<CoroutineManager>( ()=>{ return new CoroutineManager(); } );
        public static CoroutineManager Instance { get { return s_instance.Value; } }
        
        /// <summary>
        /// Contains the list of coroutines to be executed.
        /// </summary>
        private List<Stack<Coroutine>> m_coroutines = new List<Stack<Coroutine>>();

        /// <summary>
        /// The context currently being run. Used to determine if we should enqueue a coroutine
        /// to the one currently being executed if a StartCoroutine is encountered within
        /// the routine being executed.
        /// </summary>
        private int m_currentcontextidx = -1;

        private CoroutineManager() { }

        /// <summary>
        /// Compatibility function to be more compatible with the GameManager's coroutines.
        /// Runs a coroutine inside the current coroutine context.
        /// </summary>
        /// <param name="coro"></param>
        /// <returns></returns>
        public Coroutine StartCoroutine(IEnumerator<YieldInstruction> coro)
        {
            return StartCoroutine(new Coroutine(coro), false);
        }

        public Coroutine StartCoroutine(Coroutine coro, bool branch)
        {
            int contextidx = branch ? -1 : m_currentcontextidx;

            //if we're not currently in a context, or have been explicitly told to start a new one
            if (contextidx < 0)
            {
                Stack<Coroutine> stack = new Stack<Coroutine>();
                stack.Push(coro);
                m_coroutines.Add(stack);
                contextidx = m_coroutines.Count - 1;
            }

            m_coroutines[contextidx].Push(coro);

            //in case the current context is zero, and we're switching to a new one
            //we MUST set the context idx to the new value for the runtime of the new coroutine
            //and then set it back
            int tempidx = m_currentcontextidx;
            m_currentcontextidx = contextidx;
            

            coro.MoveNext();
            if (coro.FinishedYield())
            {
                m_coroutines[contextidx].Pop();
                if (m_coroutines[contextidx].Count == 0)
                    m_coroutines.RemoveAt(contextidx);
            }


            //set back the context idx
            m_currentcontextidx = tempidx;
            return coro;
        }

        /// <summary>
        /// Updates all scheduled coroutines for a single frame.
        /// </summary>
        public void Update()
        {
            try
            {
                for (int ii = 0; ii < m_coroutines.Count;ii++)
                {
                    //Exec the stack and increment
                    m_currentcontextidx = ii;
                    execStack(m_coroutines[ii]);
                    if (m_coroutines[ii].Count == 0)
                    {
                        m_coroutines.RemoveAt(ii);
                        ii--;
                    }
                }
            }
            catch (Exception ex)
            {
                DiagManager.Instance.LogError(ex);
            }
            //Make sure the context index is invalid after the update.
            // So we can tell if a foreign coroutine is being run using the coroutine manager's methods.
            m_currentcontextidx = -1;
        }

        /// <summary>
        /// Runs a single sheduled coroutine's stack.
        /// </summary>
        /// <param name="cocontext"></param>
        private void execStack(Stack<Coroutine> cocontext)
        {
            Coroutine top = cocontext.Peek();
            top.Update();
            if (top.FinishedYield())
            {
                cocontext.Pop();
                checkCoroutine(cocontext);
            }
        }

        /// <summary>
        /// Checks to see if the calling coroutine can also continue to finish.
        /// </summary>
        /// <param name="cocontext"></param>
        private void checkCoroutine(Stack<Coroutine> cocontext)
        {
            bool wantsAnother;
            do
            {
                wantsAnother = false;
                if (cocontext.Count == 0)
                    return;
                Coroutine routine = cocontext.Peek();
                routine.MoveNext();
                if (routine.FinishedYield())
                {
                    cocontext.Pop();
                    wantsAnother = true;
                }
            } while (wantsAnother);
        }


        /// <summary>
        /// Simple helper method to allow processing a function as a coroutine.
        /// </summary>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static IEnumerator<YieldInstruction> FunAsCoroutine(Action fun)
        {
            fun();
            yield break;
        }


        /// <summary>
        /// Simple helper method to allow processing a coroutine with an end action.
        /// </summary>
        /// <param name="coroutine"></param>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static IEnumerator<YieldInstruction> CoroutineWithEndAction(IEnumerator<YieldInstruction> coroutine, Action fun)
        {
            yield return CoroutineManager.Instance.StartCoroutine(coroutine);
            fun();
        }
    }
}
