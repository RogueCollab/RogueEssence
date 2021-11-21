using RogueEssence.Script;
using System;
using System.Collections.Generic;

namespace RogueEssence.Ground
{
    /// <summary>
    /// Represents a task that a ground entity should execute.
    /// Its meant to allow several entities to perform a task simultaneously, and offer a status update to the user.
    /// </summary>
    public abstract class GroundTask
    {
        /// <summary>
        /// Possible states for the task to be in.
        /// </summary>
        public enum EStatus
        {
            Waiting = 0,    //Before a task has been processed.
            Running,        //When a task is being processed.
            Completed,      //When a task finish running normally.
            Failed,         //When a task fails before completing. AKA throws an exception.
            Interupted,     //When a task is canceled before completing
        }

        /// <summary>
        /// Current status of the task. Changes as the task is processed.
        /// </summary>
        public EStatus Status { get; internal set; }

        /// <summary>
        /// When this boolean is set to true, the task will be force stopped on the next yield.
        /// </summary>
        protected bool m_bInteruptTask = false;

        /// <summary>
        /// Returns whether the task is done running, regardless of whether it errored or not.
        /// </summary>
        /// <returns></returns>
        public bool Finished()
        {
            return Status == EStatus.Completed || Status == EStatus.Failed || Status == EStatus.Interupted;
        }

        /// <summary>
        /// Return true only if the task finished with the Completed status.
        /// </summary>
        /// <returns></returns>
        public bool Completed()
        {
            return Status == EStatus.Completed;
        }

        /// <summary>
        /// Force the currently running task to stop on the next yield instruction.
        /// </summary>
        public void ForceStop()
        {
            m_bInteruptTask = true;
        }

        /// <summary>
        /// Create the enumerator to be run by the Run function.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator<YieldInstruction> CreateTaskEnumerator(GroundEntity ent);


        /// <summary>
        /// Returns a name for the task
        /// </summary>
        /// <returns></returns>
        public abstract string TaskName();

        /// <summary>
        /// Wraps the execution of the task
        /// Automatically sets the execution status.
        /// </summary>
        /// <param name="ent">Entity running the task.</param>
        /// <returns></returns>
        public virtual IEnumerator<YieldInstruction> Run(GroundEntity ent)
        {
            Status = EStatus.Running;
            var process = CreateTaskEnumerator(ent);
            bool running = false;
            Exception curex = null;
            do
            {
                //Process move next, break and keep exception if there's one.
                //! #NOTE: We can't have "yield return" inside a "try" block!
                try { running = process.MoveNext(); }
                catch (Exception ex) { curex = ex; break; }
                if (running)
                    yield return process.Current;

            } while (running && !m_bInteruptTask); //Stop if MoveNext returns false, or the task is interrupted!

            if (curex != null)
            {
                DiagManager.Instance.LogInfo(String.Format("GroundTask.Run(): Task \"{0}\" threw an exception!\n{1}", TaskName(), curex.Message));
                Status = EStatus.Failed;
            }
            else if (m_bInteruptTask)
                Status = EStatus.Interupted;
            else
                Status = EStatus.Completed;
            yield break;
        }

        /// <summary>
        /// This waits until the task has been executed. Its mainly meant to be called by the script engine via a lua yield.
        /// </summary>
        /// <returns></returns>
        public Coroutine Wait()
        {
            return new Coroutine(_Wait());
        }

        /// <summary>
        /// Internal version of the script exposed Wait function.
        /// Since Interface types like IEnumerator lose their meaning when passed through lua, it has to be wrapped in a coroutine.
        /// </summary>
        /// <returns></returns>
        private IEnumerator<YieldInstruction> _Wait()
        {
            do
            {
                yield return new WaitForFrames(1);
            } while (!Finished());
        }
    }

//=====================================================
//  GroundTask Implementations
//=====================================================
    /// <summary>
    /// Implementation of a Ground task via native code.
    /// </summary>
    public class GroundNativeTask : GroundTask
    {
        /// <summary>
        /// The task itself is a TransientScriptEvent.
        /// </summary>
        public Func<GroundEntity,IEnumerator<YieldInstruction>> Task { get; internal set; }

        /// <summary>
        /// The name assigned to this task.
        /// </summary>
        private string m_taskname;

        public GroundNativeTask(Func<GroundEntity, IEnumerator<YieldInstruction>> task, string taskname = null )
        {
            Status = EStatus.Waiting;
            Task = task;

            if (String.IsNullOrEmpty(taskname))
                m_taskname = "NativeTask#" + GetHashCode();
            else
                m_taskname = taskname;
        }

        protected override IEnumerator<YieldInstruction> CreateTaskEnumerator(GroundEntity ent)
        {
            return Task.Invoke(ent);
        }

        public override string TaskName()
        {
            return m_taskname;
        }
    }


    /// <summary>
    /// Variant of a task that runs a lua function!
    /// </summary>
    public class GroundScriptedTask : GroundTask
    {
        /// <summary>
        /// The task itself is a TransientScriptEvent.
        /// </summary>
        public TransientScriptEvent Task { get; internal set; }

        /// <summary>
        /// Constructor, takes a script event and run it as a task!
        /// </summary>
        /// <param name="ev">The TransientScriptEvent that runs the task.</param>
        public GroundScriptedTask(TransientScriptEvent ev)
        {
            Status = EStatus.Waiting;
            Task = ev;
        }

        /// <summary>
        /// Constructor, takes a lua function.
        /// </summary>
        /// <param name="fun">Lua function running the task.</param>
        public GroundScriptedTask(NLua.LuaFunction fun)
        {
            Status = EStatus.Waiting;
            Task = new TransientScriptEvent(fun);
        }

        protected override IEnumerator<YieldInstruction> CreateTaskEnumerator(GroundEntity ent)
        {
            yield return CoroutineManager.Instance.StartCoroutine(Task.Apply(ent));
        }

        public override string TaskName()
        {
            return Task.EventName();
        }
    }
}
