using System;
using RogueElements;
using System.Linq;
using System.Collections.Generic;

namespace RogueEssence.Dungeon
{
    public interface IMapLayer
    {
        IMapLayer Clone();
        void Merge(IMapLayer other);
    }

}

