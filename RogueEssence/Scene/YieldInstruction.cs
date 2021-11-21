using System;
using System.Collections.Generic;

//This file contains all classes related to the coroutine system that governs the Dungeon portion of the game

namespace RogueEssence
{
    public abstract class YieldInstruction
    {
        public abstract bool FinishedYield();
        public virtual void Update() { }
    }

    public class WaitForFrames : YieldInstruction
    {
        private long frames;
        public WaitForFrames(long frames)
        {
            this.frames = frames;
        }

        public override bool FinishedYield()
        {
            return frames <= 0;
        }

        public override void Update()
        {
            frames--;
        }
    }

    public class LuaCoroutine : Coroutine
    {
        string name;

        public LuaCoroutine(string name, IEnumerator<YieldInstruction> enumerator) : base(enumerator)
        {
            this.name = name;
        }

        public override string GetEnumeratorString()
        {
            return name;
        }
    }


    public class Coroutine : YieldInstruction
    {
        IEnumerator<YieldInstruction> enumerator;
        bool finished;
        public Coroutine(IEnumerator<YieldInstruction> enumerator)
        {
            this.enumerator = enumerator;
        }

        public void MoveNext()
        {
            bool wantsAnother;
            do
            {
                wantsAnother = false;
                
                bool hasAnother = false;
                try
                {
                    hasAnother = enumerator.MoveNext();
                }
                catch (Exception ex)
                {
                    DiagManager.Instance.LogError(ex);
                }

                if (!hasAnother)
                    finished = true;
                else
                {
                    if (enumerator.Current.FinishedYield())
                        wantsAnother = true;
                }
            } while (wantsAnother);
        }

        public override bool FinishedYield()
        {
            return finished;
        }

        public override void Update()
        {
            if (enumerator.Current != null)
            {
                enumerator.Current.Update();
                if (enumerator.Current.FinishedYield())
                    MoveNext();
            }
            else
                MoveNext();
        }

        public virtual string GetEnumeratorString()
        {
            return enumerator.ToString();
        }
    }

    public class WaitUntil : YieldInstruction
    {
        Func<bool> predicate;
        public WaitUntil(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public override bool FinishedYield()
        {
            return predicate();
        }
    }

    public class WaitWhile : YieldInstruction
    {
        Func<bool> predicate;
        public WaitWhile(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public override bool FinishedYield()
        {
            return !predicate();
        }
    }
}
