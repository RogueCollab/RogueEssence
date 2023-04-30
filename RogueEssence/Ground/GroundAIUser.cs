using System;

namespace RogueEssence.Ground
{
    /// <summary>
    /// A base class for anything using an AI.
    /// The AI requires the entity to use Tasks, so the base class is BaseTaskUser.
    /// </summary>
    [Serializable]
    public abstract class GroundAIUser : BaseTaskUser
    {
        /// <summary>
        /// AI container
        /// </summary>
        protected virtual GroundAI AI
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the entity is busy interacting, and should act accordingly
        /// </summary>
        public virtual bool IsInteracting { get; protected set; }

        /// <summary>
        /// Whether the AI is enabled or disabled for this entity
        /// </summary>
        public virtual bool AIEnabled { get; set; }

        protected GroundAIUser() : base() { }

        protected GroundAIUser(GroundAIUser other) : base(other)
        { }

        /// <summary>
        /// Sets the AI to use.
        /// </summary>
        /// <param name="ai"></param>
        public virtual void SetAI(GroundAI ai)
        {
            AI = ai;
            AI.EntityPointer = this;
        }

        /// <summary>
        /// Sets the AI state.
        /// </summary>
        /// <param name="ai"></param>
        public virtual void SetAIState(string state)
        {
            AI.ForceState(state);
        }

        /// <summary>
        /// Whether the entity has currently an AI set!
        /// </summary>
        /// <returns></returns>
        public virtual bool hasAI()
        {
            return AI != null;
        }

        public virtual bool ShouldAIRun()
        {
            return AIEnabled/* && GroundAI.GlobalAIDisabled*/;
        }

        /// <summary>
        /// This should be called on map init so the AI can be initialized
        /// </summary>
        public override void OnMapInit()
        {
            if (AI != null)
                AI.OnMapInit();
            base.OnMapInit();
        }

        /// <summary>
        /// This method is reponsible for handling AI processing.
        /// The states also handle tasks themselves
        /// </summary>
        /// <returns></returns>
        public virtual void UpdateAI()
        {
            AI.UpdateAI();
        }

        /// <summary>
        /// Reimplemented think method to take into account AI.
        /// Think shouldn't be blocking by definition. Anything blocking should be run as a task, or game action.
        /// </summary>
        public virtual void Think()
        {
            if (hasAI() && ShouldAIRun())
                UpdateAI();
        }

        public override void DoCleanup()
        {
            base.DoCleanup();
            if (hasAI())
                AI.DoCleanup();
        }
    }
}
