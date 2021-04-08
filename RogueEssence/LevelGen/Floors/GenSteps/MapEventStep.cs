using System;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public class CheckEventStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public List<SingleCharEvent> CheckEvents;

        public CheckEventStep()
        {
            CheckEvents = new List<SingleCharEvent>();
        }

        public override void Apply(T map)
        {
            foreach (SingleCharEvent effect in CheckEvents)
                map.Map.CheckEvents.Add((SingleCharEvent)effect.Clone());
        }
    }
    [Serializable]
    public class MapEffectStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public ActiveEffect Effect;

        public MapEffectStep()
        {
            Effect = new ActiveEffect();
        }

        public MapEffectStep(ActiveEffect effect)
        {
            Effect = new ActiveEffect();
        }

        public override void Apply(T map)
        {
            map.Map.MapEffect = Effect;
        }
    }
}
