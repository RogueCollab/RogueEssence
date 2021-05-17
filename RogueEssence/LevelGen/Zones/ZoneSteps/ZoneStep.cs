using System;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class ZoneStep
    {
        /// <summary>
        /// Shallow copy + Initialize any runtime variables
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public abstract ZoneStep Instantiate(ulong seed);
        public abstract void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue);
    }
}
