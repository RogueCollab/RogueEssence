using System;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueElements;

namespace RogueEssence.LevelGen
{
    [Serializable]
    public abstract class MobSpawnExtra : Dev.EditorData
    {
        public abstract MobSpawnExtra Copy();
        public abstract void ApplyFeature(IRandom rand, Character newChar);
    }


    [Serializable]
    public class MobSpawnStatus : MobSpawnExtra
    {
        public SpawnList<StatusEffect> Statuses;

        public MobSpawnStatus()
        {
            Statuses = new SpawnList<StatusEffect>();
        }
        public MobSpawnStatus(MobSpawnStatus other) : this()
        {
            for (int ii = 0; ii < other.Statuses.Count; ii++)
                Statuses.Add(other.Statuses.GetSpawn(ii).Clone(), other.Statuses.GetSpawnRate(ii));
        }
        public override MobSpawnExtra Copy() { return new MobSpawnStatus(this); }

        public override void ApplyFeature(IRandom rand, Character newChar)
        {
            StatusEffect status = Statuses.Pick(rand).Clone();//Clone Use Case; convert to Instantiate?
            status.LoadFromData();
            StatusData entry = (StatusData)status.GetData();
            if (!entry.Targeted)//no targeted statuses allowed
            {
                //need to also add the additional status states
                newChar.StatusEffects.Add(status.ID, status);
            }
        }
    }

}
