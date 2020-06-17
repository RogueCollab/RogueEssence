using System;
using System.Collections.Generic;

namespace RogueEssence.Ground
{
    /// <summary>
    /// class to implement to add basic task handling to an entity
    /// </summary>
    [Serializable]
    public abstract class BaseTaskUser : GroundEntity
    {
        //================================================
        //  Task Handling
        //================================================
        /// <summary>
        /// Represents how this entity's think function should be handled!
        /// </summary>
        public enum EThink
        {
            Never,      //The think function is never called for this entity
            //Once,       //The think function is called only once after the entity is spawned
            Always,     //The think function is called every so often
        }

        /// <summary>
        /// Contains the type of thinking this entity does
        /// </summary>
        public virtual EThink ThinkType { get; set; }

        /// <summary>
        /// Current task
        /// </summary>
        [NonSerialized] private GroundTask Task = null;

        /// <summary>
        /// The coroutine returning the current task's state.
        /// </summary>
        //[NonSerialized] private Coroutine TaskState = null;

        enum ETaskState
        {
            None = -1,
            Running = 0,
            Complete = 1,
        }
        [NonSerialized] private ETaskState TaskState = ETaskState.None;

        /// <summary>
        /// Set the entity's current task to the one specified in parameters.
        /// Only sets the task if the current one is finished!
        /// </summary>
        /// <param name="task">New task to set as current task</param>
        /// <returns>Returns false if the entity is still running a task! And true if the task was properly set!</returns>
        public virtual bool SetTask(GroundTask task)
        {
            if (Task != null && !Task.Finished())
                return false;

            Task = task;
            return true;
        }

        /// <summary>
        /// This function waits until the current task is done before setting the new task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public virtual IEnumerator<YieldInstruction> WaitSetTask(GroundTask task)
        {
            yield return new WaitWhile(() => { return Task != null && !Task.Finished(); });
            Task = task;
        }

        /// <summary>
        /// Return the current task, or null if no task is set!
        /// </summary>
        /// <returns></returns>
        public virtual GroundTask CurrentTask()
        {
            return Task;
        }

        /// <summary>
        /// Update tasks specifically.
        /// </summary>
        public virtual void UpdateTask()
        {
            if (Task == null)
                return;

            if (TaskState == ETaskState.None)
                CoroutineManager.Instance.StartCoroutine(runToCompletion());
            else if (TaskState == ETaskState.Complete)
                TaskState = ETaskState.None;

            if (Task.Finished())
                Task = null;
        }

        private IEnumerator<YieldInstruction> runToCompletion()
        {
            TaskState = ETaskState.Running;

            yield return CoroutineManager.Instance.StartCoroutine(Task.Run(this));

            TaskState = ETaskState.Complete;
        }

        /// <summary>
        /// Think method. Handles running tasks.
        /// Think shouldn't be blocking by definition. We defer actually executing tasks/coroutines to the GameManager.
        /// </summary>
        public virtual void Think()
        {
            UpdateTask();
        }
    }
}
