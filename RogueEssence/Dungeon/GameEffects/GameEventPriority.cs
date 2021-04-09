using System;
using RogueElements;

namespace RogueEssence.Dungeon
{

    public class GameEventPriority : IComparable<GameEventPriority>
    {
        public static readonly Priority USER_PORT_PRIORITY = new Priority(-2);
        public static readonly Priority TARGET_PORT_PRIORITY = new Priority(-1);

        public enum EventCause
        {
            None,
            Skill,
            MapState,
            Tile,
            Terrain,
            Status,
            Equip,
            Intrinsic
        }

        public Priority Priority;//dev specified; lowest first
        public Priority PortPriority;//user (-2), target (-1), everyone else (0+)
        public EventCause TypeID;//tiebreaker order of enum
        public int ID;//tiebreaker: the ID of the owning passive/battle effect
        public int ListIndex;//last tiebreaker: location in the list

        public GameEventPriority(Priority priority, Priority portPriority, EventCause typeId, int id, int listIndex)
        {
            Priority = priority;
            PortPriority = portPriority;
            TypeID = typeId;
            ID = id;
            ListIndex = listIndex;
        }

        public int CompareTo(GameEventPriority other)
        {
            // If other is not a valid object reference, this instance is greater. 
            if (other == null) return 1;

            int priority = Priority.CompareTo(other.Priority);
            if (priority != 0)
                return priority;

            int portPriority = PortPriority.CompareTo(other.PortPriority);
            if (portPriority != 0)
                return portPriority;

            int typeId = TypeID.CompareTo(other.TypeID);
            if (typeId != 0)
                return typeId;

            int id = ID.CompareTo(other.ID);
            if (id != 0)
                return id;

            int index = ListIndex.CompareTo(other.ListIndex);
            if (index != 0)
                return index;

            return 0;
        }


    }
}
