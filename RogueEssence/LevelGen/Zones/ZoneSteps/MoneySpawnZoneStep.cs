using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Dev;
using RogueEssence.Data;
using System.Runtime.Serialization;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates the table of items to spawn on all floors
    /// </summary>
    [Serializable]
    public class MoneySpawnZoneStep : ZoneStep
    {
        public Priority Priority;

        [Dev.RangeBorder(0, false, true)]
        public RandRange StartAmount;

        [Dev.RangeBorder(0, false, true)]
        public RandRange AddAmount;

        [StringTypeConstraint(0, typeof(ModGenState))]
        public List<FlagType> ModStates;

        [NonSerialized]
        private int chosenStart;
        [NonSerialized]
        private int chosenAdd;

        public MoneySpawnZoneStep()
        {
            ModStates = new List<FlagType>();
        }

        public MoneySpawnZoneStep(Priority priority, RandRange start, RandRange add)
        {
            ModStates = new List<FlagType>();
            Priority = priority;
            StartAmount = start;
            AddAmount = add;
        }

        protected MoneySpawnZoneStep(MoneySpawnZoneStep other, ulong seed) : this()
        {
            StartAmount = other.StartAmount;
            AddAmount = other.AddAmount;
            Priority = other.Priority;
            ModStates.AddRange(other.ModStates);

            ReRandom rand = new ReRandom(seed);
            chosenStart = StartAmount.Pick(rand);
            chosenAdd = AddAmount.Pick(rand);
        }
        public override ZoneStep Instantiate(ulong seed) { return new MoneySpawnZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            RandRange amount = new RandRange(chosenStart + chosenAdd * zoneContext.CurrentID);

            GameProgress progress = DataManager.Instance.Save;
            if (progress != null && progress.ActiveTeam != null)
            {
                int totalMod = 0;
                foreach (Character chara in progress.ActiveTeam.Players)
                {
                    foreach (FlagType state in ModStates)
                    {
                        CharState foundState;
                        if (chara.CharStates.TryGet(state.FullType, out foundState))
                            totalMod += ((ModGenState)foundState).Mod;
                    }
                }
                amount.Min = amount.Min * (100 + totalMod) / 100;
                amount.Max = amount.Max * (100 + totalMod) / 100;
            }

            queue.Enqueue(Priority, new MoneySpawnStep<BaseMapGenContext>(amount));
        }

        //TODO: Created v0.5.2, delete on v0.6.1
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (ModStates == null)
                ModStates = new List<FlagType>();
        }
    }
}
