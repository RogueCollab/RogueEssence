using System;
using RogueEssence.Data;
using RogueElements;

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameEventOwner
    {
        public abstract GameEventPriority.EventCause GetEventCause();

        public abstract int GetID();
        public abstract string GetDisplayName();

        public void AddEventsToQueue<T>(StablePriorityQueue<GameEventPriority, EventQueueElement<T>> queue, Priority maxPriority, ref Priority nextPriority, PriorityList<T> effectList, Character targetChar) where T : GameEvent
        {
            foreach(Priority priority in effectList.GetPriorities())
            {
                //if an item has the same priority variable as the nextPriority, enqueue it
                //if an item has a higher priority variable than nextPriority, ignore it
                //if an item has a lower priority variable than nextPriority, check against maxPriority
                for (int ii = 0; ii < effectList.GetCountAtPriority(priority); ii++)
                {
                    if (priority == nextPriority)
                    {
                        GameEventPriority gameEventPriority = new GameEventPriority(priority, GameEventPriority.USER_PORT_PRIORITY, GetEventCause(), GetID(), ii);
                        EventQueueElement<T> eventQueueElement = new EventQueueElement<T>(this, null, effectList.Get(priority, ii), targetChar);
                        queue.Enqueue(gameEventPriority, eventQueueElement);
                    }
                    else if (priority < nextPriority || nextPriority == Priority.Invalid)
                    {
                        //if the item has a lower priority variable than maxPriority, ignore it
                        //if the item has a higher priority variable than maxPriority, clear the queue and add the new item
                        if (priority > maxPriority || maxPriority == Priority.Invalid)
                        {
                            nextPriority = priority;
                            queue.Clear();
                            GameEventPriority gameEventPriority = new GameEventPriority(priority, GameEventPriority.USER_PORT_PRIORITY, GetEventCause(), GetID(), ii);
                            EventQueueElement<T> eventQueueElement = new EventQueueElement<T>(this, null, effectList.Get(priority, ii), targetChar);
                            queue.Enqueue(gameEventPriority, eventQueueElement);
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

    public class EventQueueElement<T>
        where T : GameEvent
    {
        public GameEventOwner Owner;
        public Character OwnerChar;
        public T Event;
        public Character TargetChar;

        public EventQueueElement(GameEventOwner owner, Character ownerChar, T newEvent, Character targetChar)
        {
            Owner = owner;
            OwnerChar = ownerChar;
            Event = newEvent;
            TargetChar = targetChar;
        }
    }

    public class PassiveContext
    {
        public PassiveActive Passive;
        public PassiveData EventData;
        public Priority PortPriority;
        public Character EventChar;

        public PassiveContext(PassiveActive passive, PassiveData passiveEntry, Priority portPriority, Character effectChar)
        {
            Passive = passive;
            EventData = passiveEntry;
            PortPriority = portPriority;
            EventChar = effectChar;
        }


        public void AddEventsToQueue<T>(StablePriorityQueue<GameEventPriority, EventQueueElement<T>> queue, Priority maxPriority, ref Priority nextPriority, PriorityList<T> effectList, Character targetChar) where T : GameEvent
        {
            foreach(Priority priority in effectList.GetPriorities())
            {
                //if an item has the same priority variable as the nextPriority, enqueue it
                //if an item has a higher priority variable than nextPriority, ignore it
                //if an item has a lower priority variable than nextPriority, check against maxPriority
                for (int ii = 0; ii < effectList.GetCountAtPriority(priority); ii++)
                {
                    if (priority == nextPriority)
                    {
                        GameEventPriority gameEventPriority = new GameEventPriority(priority, PortPriority, Passive.GetEventCause(), Passive.GetID(), ii);
                        EventQueueElement<T> queueElement = new EventQueueElement<T>(Passive, EventChar, effectList.Get(priority, ii), targetChar);
                        queue.Enqueue(gameEventPriority, queueElement);
                    }
                    else if (priority < nextPriority || nextPriority == Priority.Invalid)
                    {
                        //if the item has a lower priority variable than maxPriority, ignore it
                        //if the item has an equal or higher priority variable than maxPriority, clear the queue and add the new item
                        if (priority > maxPriority || maxPriority == Priority.Invalid)
                        {
                            nextPriority = priority;
                            queue.Clear();
                            GameEventPriority gameEventPriority = new GameEventPriority(priority, PortPriority, Passive.GetEventCause(), Passive.GetID(), ii);
                            EventQueueElement<T> queueElement = new EventQueueElement<T>(Passive, EventChar, effectList.Get(priority, ii), targetChar);
                            queue.Enqueue(gameEventPriority, queueElement);
                        }
                    }
                }
            }
        }
    }

}
