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
        /// <summary>
        /// At what point in the map gen process to run the money spawning in.
        /// </summary>
        public Priority Priority;

        /// <summary>
        /// The amount of money spawned on the first floor.
        /// </summary>
        [Dev.RangeBorder(0, false, true)]
        public RandRange StartAmount;

        /// <summary>
        /// The amount of money that is added on each increasing floor.
        /// </summary>
        [Dev.RangeBorder(0, false, true)]
        public RandRange AddAmount;

        /// <summary>
        /// Flags from the player's passives that will affect the money spawned.
        /// If a player enters a floor and is carrying an item, intrinsic, etc. that has a ModGenState listed here,
        /// The amount of money spawned will be increased by the ModGenState's value.
        /// </summary>
        [StringTypeConstraint(1, typeof(ModGenState))]
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
    }
}
