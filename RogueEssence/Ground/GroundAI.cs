using System;

namespace RogueEssence.Ground
{
    /// <summary>
    /// Base class implementation of GroundMode AI
    /// </summary>
    [Serializable]
    public abstract class GroundAI
    {
        /// <summary>
        /// State index the AI is currently in
        /// </summary>
        public virtual string CurrentState { get; protected set; }

        /// <summary>
        /// Pointer to the GroundEntity that inherits this.
        /// </summary>
        [NonSerialized]
        public GroundEntity EntityPointer;

        /// <summary>
        /// This method handles initializing the AI, if needed, when a map is run.
        /// </summary>
        public abstract void OnMapInit();

        /// <summary>
        /// The AI will perform all its necessary operations in this method.
        /// </summary>
        /// <returns></returns>
        public abstract void UpdateAI();

        /// <summary>
        /// Force the AI to change to the specified state if it exists
        /// </summary>
        /// <param name="statename"></param>
        public abstract void ForceState(string statename);
    }
}
