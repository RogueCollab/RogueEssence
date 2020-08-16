using System;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence
{
    [Serializable]
    public class FillImpassableStep<T> : GenStep<T>
        where T : class, ITiledGenContext
    {
        public FillImpassableStep()
        {
        }

        public override void Apply(T map)
        {
            //TODO: find all fully unreachable tiles and fill in with impassable

        }
    }
}
