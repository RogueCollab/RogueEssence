using System;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Sets the map's own events.  These events work similarly to the Universal Event, which works game-wide.
    /// These events work map-wide.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapEffectStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The object containing the events.
        /// </summary>
        [Dev.SubGroup]
        public ActiveEffect Effect;

        public MapEffectStep()
        {
            Effect = new ActiveEffect();
        }

        public MapEffectStep(ActiveEffect effect)
        {
            Effect = effect;
        }

        public override void Apply(T map)
        {
            map.Map.MapEffect.AddOther(Effect);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().GetFormattedTypeName(), this.Effect.GetTotalCount());
        }
    }
}
