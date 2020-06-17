using System;
using RogueEssence.Data;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameEventOwner : Dev.EditorData
    {
        public abstract GameEventPriority.EventCause GetEventCause();

        public abstract int GetID();
        public abstract string GetName();

        public void AddEventsToQueue<T>(StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, T>> queue, int maxPriority, ref int nextPriority, PriorityList<T> effectList) where T : GameEvent
        {
            foreach(int priority in effectList.GetPriorities())
            {
                //if an item has the same priority variable as the nextPriority, enqueue it
                //if an item has a higher priority variable than nextPriority, ignore it
                //if an item has a lower priority variable than nextPriority, check against maxPriority
                for (int ii = 0; ii < effectList.GetCountAtPriority(priority); ii++)
                {
                    if (priority == nextPriority)
                        queue.Enqueue(new GameEventPriority(priority, GameEventPriority.USER_PORT_PRIORITY, GetEventCause(), GetID(), ii), new Tuple<GameEventOwner, Character, T>(this, null, effectList.Get(priority, ii)));
                    else if (priority < nextPriority)
                    {
                        //if the item has a lower priority variable than maxPriority, ignore it
                        //if the item has an equal or higher priority variable than maxPriority, clear the queue and add the new item
                        if (priority >= maxPriority)
                        {
                            nextPriority = priority;
                            queue.Clear();
                            queue.Enqueue(new GameEventPriority(priority, GameEventPriority.USER_PORT_PRIORITY, GetEventCause(), GetID(), ii), new Tuple<GameEventOwner, Character, T>(this, null, effectList.Get(priority, ii)));
                        }
                    }
                }
            }
        }
    }


    [Serializable]
    public abstract class PassiveActive : GameEventOwner
    {
        public override int GetID() { return ID; }
        public abstract PassiveData GetData();
        public int ID;

        public PassiveActive()
        {
            ID = -1;
        }
        public PassiveActive(PassiveActive other)
        {
            ID = other.ID;
        }
    }

    public class PassiveContext
    {
        public PassiveActive Passive;
        public PassiveData EventData;
        public int PortPriority;
        public Character EventChar;

        public PassiveContext(PassiveActive passive, PassiveData passiveEntry, int portPriority, Character effectChar)
        {
            Passive = passive;
            EventData = passiveEntry;
            PortPriority = portPriority;
            EventChar = effectChar;
        }


        public void AddEffectsToQueue<T>(StablePriorityQueue<GameEventPriority, Tuple<GameEventOwner, Character, T>> queue, int maxPriority, ref int nextPriority, PriorityList<T> effectList) where T : GameEvent
        {
            foreach(int priority in effectList.GetPriorities())
            {
                //if an item has the same priority variable as the nextPriority, enqueue it
                //if an item has a higher priority variable than nextPriority, ignore it
                //if an item has a lower priority variable than nextPriority, check against maxPriority
                for (int ii = 0; ii < effectList.GetCountAtPriority(priority); ii++)
                {
                    if (priority == nextPriority)
                        queue.Enqueue(new GameEventPriority(priority, PortPriority, Passive.GetEventCause(), Passive.GetID(), ii), new Tuple<GameEventOwner, Character, T>(Passive, EventChar, effectList.Get(priority, ii)));
                    else if (priority < nextPriority)
                    {
                        //if the item has a lower priority variable than maxPriority, ignore it
                        //if the item has an equal or higher priority variable than maxPriority, clear the queue and add the new item
                        if (priority >= maxPriority)
                        {
                            nextPriority = priority;
                            queue.Clear();
                            queue.Enqueue(new GameEventPriority(priority, PortPriority, Passive.GetEventCause(), Passive.GetID(), ii), new Tuple<GameEventOwner, Character, T>(Passive, EventChar, effectList.Get(priority, ii)));
                        }
                    }
                }
            }
        }
    }

}
