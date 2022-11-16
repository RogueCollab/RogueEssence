using System;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence
{
    /// <summary>
    /// Finds all fully unreachable tiles that aren't impassable and turns them impassable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
