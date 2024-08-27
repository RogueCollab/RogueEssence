using System;

namespace RogueEssence.Dungeon
{
    public class AbortStatus
    {
        public bool Cancel;

        public AbortStatus() { }
        public AbortStatus(bool cancel) { Cancel = cancel; }
        public AbortStatus(AbortStatus other) { Cancel = other.Cancel; }
    }

    public abstract class UserTargetGameContext : GameContext
    {
        /// <summary>
        /// The character that the action is targeted at
        /// </summary>
        public Character Target { get; set; }

        public UserTargetGameContext() : base()
        {
        }

        protected UserTargetGameContext(UserTargetGameContext other) : base(other)
        {
            Target = other.Target;
        }
    }

    public abstract class GameContext
    {
        /// <summary>
        /// Contains contextual info to be passed along the GameContext, used by GameEvents
        /// </summary>
        public StateCollection<ContextState> ContextStates;

        /// <summary>
        /// The character that is performing the action
        /// </summary>
        public Character User { get; set; }

        /// <summary>
        /// Whether the action should be canceled or not.
        /// </summary>
        public AbortStatus CancelState;


        protected GameContext()
        {
            ContextStates = new StateCollection<ContextState>();
            CancelState = new AbortStatus();
        }

        protected GameContext(GameContext other)
        {
            ContextStates = other.ContextStates.Clone();

            User = other.User;
            CancelState = other.CancelState;
        }


        public int GetContextStateInt<T>(int defaultVal) where T : ContextIntState
        {
            return GetContextStateInt(typeof(T), defaultVal);
        }
        public int GetContextStateInt(Type type, int defaultVal)
        {
            ContextIntState countState = (ContextIntState)ContextStates.GetWithDefault(type);
            if (countState == null)
                return defaultVal;
            else
                return countState.Count;
        }

        public void AddContextStateInt<T>(int addedVal) where T : ContextIntState
        {
            AddContextStateInt(typeof(T), addedVal);
        }
        public void AddContextStateInt(Type type, int addedVal)
        {
            ContextIntState countState = (ContextIntState)ContextStates.GetWithDefault(type);
            if (countState == null)
            {
                ContextIntState newCount = (ContextIntState)Activator.CreateInstance(type);
                newCount.Count = addedVal;
                ContextStates.Set(newCount);
            }
            else
                countState.Count += addedVal;
        }

    }
}
