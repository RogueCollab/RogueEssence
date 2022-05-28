using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dev;
using RogueEssence.Dungeon;

namespace RogueEssence.LevelGen
{
    /// <summary>
    /// Spreads a map gen step randomly across the dungeon segment.
    /// </summary>
    [Serializable]
    public class SpreadStepZoneStep : ZoneStep
    {
        /// <summary>
        /// Determines how many floors to distribute the step to, and how spread apart they are.
        /// </summary>
        public SpreadPlanBase SpreadPlan;

        /// <summary>
        /// The steps to distribute.
        /// </summary>
        public IRandPicker<IGenPriority> Spawns;

        /// <summary>
        /// Flags from the player's passives that will affect the appearance rate of the step.
        /// </summary>
        [StringTypeConstraint(0, typeof(ModGenState))]
        public List<FlagType> ModStates;

        [NonSerialized]
        public List<IGenPriority> DropItems;

        public SpreadStepZoneStep()
        {
            ModStates = new List<FlagType>();
        }

        public SpreadStepZoneStep(SpreadPlanBase plan, IRandPicker<IGenPriority> spawns) : this()
        {
            SpreadPlan = plan;
            Spawns = spawns;
        }

        protected SpreadStepZoneStep(SpreadStepZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns.CopyState();
            SpreadPlan = other.SpreadPlan.Instantiate(seed);
            ModStates.AddRange(other.ModStates);

            DropItems = new List<IGenPriority>();

            //Other SpredStep classes choose which step to place on which floor on the fly, but this one needs care, due to the potential of CanPick changing state
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                if (!Spawns.CanPick)
                    break;

                ReRandom rand = new ReRandom(seed);
                IGenPriority genStep = Spawns.Pick(rand);
                DropItems.Add(genStep);
            }
        }
        public override ZoneStep Instantiate(ulong seed) { return new SpreadStepZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            bool added = false;
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                if (SpreadPlan.DropPoints[ii] != zoneContext.CurrentID)
                    continue;
                if (ii >= DropItems.Count)
                    continue;
                IGenPriority genStep = DropItems[ii];
                queue.Enqueue(genStep.Priority, genStep.GetItem());
                added = true;
            }

            if (added)
                return;

            GameProgress progress = DataManager.Instance.Save;
            if (progress != null && progress.ActiveTeam != null)
            {
                if (Spawns.CanPick)
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
                        IGenPriority genStep = Spawns.Pick(context.Rand);
                        queue.Enqueue(genStep.Priority, genStep.GetItem());
                        return;
                    }
                }
            }
        }

        public override string ToString()
        {
            int count = 0;
            if (Spawns != null)
            {
                foreach (IGenPriority gen in Spawns.EnumerateOutcomes())
                    count++;
            }
            return string.Format("{0}[{1}]", this.GetType().Name, count);
        }

        //TODO: Created v0.5.2, delete on v0.6.1
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (ModStates == null)
                ModStates = new List<FlagType>();
        }
    }

    /// <summary>
    /// Spreads a map gen step randomly across the dungeon segment, allowing precise control over the spawn rate across different floors.
    /// </summary>
    [Serializable]
    public class SpreadStepRangeZoneStep : ZoneStep
    {
        /// <summary>
        /// Determines how many floors to distribute the step to, and how spread apart they are.
        /// </summary>
        public SpreadPlanBase SpreadPlan;

        /// <summary>
        /// The steps to distribute.  Probabilities can be customized across floors.
        /// </summary>
        public SpawnRangeList<IGenPriority> Spawns;

        /// <summary>
        /// Flags from the player's passives that will affect the appearance rate of the step.
        /// </summary>
        [StringTypeConstraint(0, typeof(ModGenState))]
        public List<FlagType> ModStates;

        [NonSerialized]
        public List<IGenPriority> DropItems;
        //spreads an item through the floors
        //ensures that the space in floors between occurrences is kept tame
        public SpreadStepRangeZoneStep()
        {
            ModStates = new List<FlagType>();
        }

        public SpreadStepRangeZoneStep(SpreadPlanBase plan, SpawnRangeList<IGenPriority> spawns) : this()
        {
            SpreadPlan = plan;
            Spawns = spawns;
        }

        protected SpreadStepRangeZoneStep(SpreadStepRangeZoneStep other, ulong seed) : this()
        {
            Spawns = other.Spawns.CopyState();
            SpreadPlan = other.SpreadPlan.Instantiate(seed);
            ModStates.AddRange(other.ModStates);

            DropItems = new List<IGenPriority>();

            //Other SpredStep classes choose which step to place on which floor on the fly, but this one needs care, due to the potential of CanPick changing state
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                int floor = SpreadPlan.DropPoints[ii];
                SpawnList<IGenPriority> spawnList = Spawns.GetSpawnList(floor);
                if (!spawnList.CanPick)
                    continue;

                ReRandom rand = new ReRandom(seed);
                IGenPriority genStep = spawnList.Pick(rand);
                DropItems.Add(genStep);
            }
        }
        public override ZoneStep Instantiate(ulong seed) { return new SpreadStepRangeZoneStep(this, seed); }


        public override void Apply(ZoneGenContext zoneContext, IGenContext context, StablePriorityQueue<Priority, IGenStep> queue)
        {
            bool added = false;
            for (int ii = 0; ii < SpreadPlan.DropPoints.Count; ii++)
            {
                if (SpreadPlan.DropPoints[ii] != zoneContext.CurrentID)
                    continue;
                if (ii >= DropItems.Count)
                    continue;
                IGenPriority genStep = DropItems[ii];
                queue.Enqueue(genStep.Priority, genStep.GetItem());
                added = true;
            }

            if (added)
                return;

            GameProgress progress = DataManager.Instance.Save;
            if (progress != null && progress.ActiveTeam != null)
            {
                SpawnList<IGenPriority> spawnList = Spawns.GetSpawnList(zoneContext.CurrentID);
                if (spawnList.CanPick)
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
                        IGenPriority genStep = spawnList.Pick(context.Rand);
                        queue.Enqueue(genStep.Priority, genStep.GetItem());
                        return;
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.GetType().Name, Spawns.Count.ToString());
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
