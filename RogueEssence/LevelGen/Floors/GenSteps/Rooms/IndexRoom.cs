using RogueElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// A component that marks a room with a numeric value that can be referenced later.
    /// </summary>
    [Serializable]
    public class IndexRoom : RoomComponent
    {
        /// <summary>
        /// The index to mark the room with.
        /// </summary>
        public int Index;
        public IndexRoom() { }
        public IndexRoom(int index) { Index = index; }
        public IndexRoom(IndexRoom other) { Index = other.Index; }
        public override RoomComponent Clone() { return new IndexRoom(this); }

        public override string ToString()
        {
            return String.Format("Indexed: {0}", Index);
        }
    }
}
