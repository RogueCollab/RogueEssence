using RogueElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class ImmutableRoom : RoomComponent
    {

        public override RoomComponent Clone() { return new ImmutableRoom(); }

        public override string ToString()
        {
            return "Immutable";
        }
    }
}
