using System;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class MobSpawnCheck
    {
        public abstract MobSpawnCheck Copy();
        public abstract bool CanSpawn();
    }

}
