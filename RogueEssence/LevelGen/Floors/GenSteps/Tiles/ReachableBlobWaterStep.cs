using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Creates blobs of water using cellular automata, and places them around the map.
    /// The water must be reachable from the stairs.
    /// WIP DOES NOT WORK
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ReachableBlobWaterStep<T> : BlobWaterStep<T> where T : class, ITiledGenContext
    {

        public ReachableBlobWaterStep() { }
        
        public ReachableBlobWaterStep(RandRange blobs, ITile terrain, ITerrainStencil<T> stencil, int minScale, RandRange startScale) : base(blobs, terrain, stencil, minScale, startScale)
        {
        }
        
        protected override bool AttemptBlob(T map, BlobMap blobMap, int blobIdx)
        {
            //TODO: blob can be placed if it does not touch the mainland
            //but it does touch water that touches the mainland
            return false;
        }
    }
}
