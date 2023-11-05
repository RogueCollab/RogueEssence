using RogueElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class IndexRoom : RoomComponent
    {
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
