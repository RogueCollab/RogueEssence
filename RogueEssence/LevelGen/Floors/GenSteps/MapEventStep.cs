using System;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class MapEffectStep<T> : GenStep<T> where T : BaseMapGenContext
    {
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
            return string.Format("{0}[{1}]", this.GetType().Name, this.Effect.GetTotalCount());
        }
    }
}
