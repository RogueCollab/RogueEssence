using System;
using RogueElements;
using System.Collections.Generic;

namespace RogueEssence
{
    /// <summary>
    /// Removes extraneous unbreakable wall tiles by turning them into regular wall tiles.
    /// THIS DOES NOT WORK YET
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MinimizeBarrierStep<T> : GenStep<T>
        where T : class, ITiledGenContext
    {
        public MinimizeBarrierStep()
        {
        }

        public override void Apply(T map)
        {
            //TODO: convert all unbreakable tiles to normal wall tiles, based on the following criteria:
            //that the unbreakable tile is not a chokepoint
            //this will be applied recursively until all tiles are chokepoints

            //iterate the map to find all unbreakable, non-chokepoint tiles once, put in a queue
            //for each element in the list, convert the tile, and add all adjacent non-chokepoint tiles to the queue (must check for already-traversed state)
        }
    }
}
