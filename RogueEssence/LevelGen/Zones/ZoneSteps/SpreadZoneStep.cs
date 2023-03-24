using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Generates specific occurrences randomly across the whole dungeon segment.
    /// </summary>
    [Serializable]
    public abstract class SpreadZoneStep : ZoneStep
    {
        /// <summary>
        /// Determines how many floors to distribute the step to, and how spread apart they are.
        /// </summary>
        public SpreadPlanBase SpreadPlan;

        /// <summary>
        /// Flags from the player's passives that will affect the appearance rate of the step.
        /// If a player enters a floor and is carrying an item, intrinsic, etc. that has a ModGenState listed here,
        /// The chance of the step appearing will be increased by the ModGenState's value.
        /// </summary>
        [StringTypeConstraint(1, typeof(ModGenState))]
        public List<FlagType> ModStates;

        public SpreadZoneStep()
        {
            ModStates = new List<FlagType>();
        }
        public SpreadZoneStep(SpreadPlanBase plan)
        {
            ModStates = new List<FlagType>();
            SpreadPlan = plan;
        }

        protected SpreadZoneStep(SpreadZoneStep other, ulong seed)
        {
            ModStates = new List<FlagType>();
            ModStates.AddRange(other.ModStates);
            SpreadPlan = other.SpreadPlan.Instantiate(seed);
        }

        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            //find the first postproc that is a GridRoom postproc and add this to its special rooms
            //NOTE: if a room-based generator is not found as the generation step, it will just skip this floor but treat it as though it was placed.
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                if (SpreadPlan.DropPoints[ii] != zoneContext.CurrentID)
                    continue;

                bool applied = ApplyToFloor(zoneContext, context, queue, ii);

                if (!applied)
                {
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
                        if (context.Rand.Next(100) < totalMod)
                        {
                            ApplyToFloor(zoneContext, context, queue, -1);
                            return;
                        }
                    }
                }
            }
        }

        protected abstract bool ApplyToFloor(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue, int dropIdx);
    }
}
