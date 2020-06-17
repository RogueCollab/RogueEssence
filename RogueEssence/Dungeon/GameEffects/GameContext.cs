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

    public abstract class GameContext
    {
        //passes contextual info
        public StateCollection<ContextState> ContextStates;
        public Character Target { get; set; }
        public Character User { get; set; }
        
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
            Target = other.Target;
            CancelState = other.CancelState;
        }


        public int GetContextStateInt<T>(int defaultVal) where T : ContextIntState
        {
            ContextIntState countState = ContextStates.Get<T>();
            if (countState == null)
                return defaultVal;
            else
                return countState.Count;
        }

        public void AddContextStateInt<T>(int addedVal) where T : ContextIntState
        {
            ContextIntState countState = ContextStates.Get<T>();
            if (countState == null)
            {
                T newCount = (T)Activator.CreateInstance(typeof(T));
                newCount.Count = addedVal;
                ContextStates.Set(newCount);
            }
            else
                countState.Count += addedVal;
        }

    }
}
