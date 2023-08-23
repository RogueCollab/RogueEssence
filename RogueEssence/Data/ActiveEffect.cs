using System;
using System.Collections.Generic;
using RogueEssence.Dungeon;
using RogueElements;
using RogueEssence.LevelGen;
using RogueEssence.Dev;

namespace RogueEssence.Data
{
    [Serializable]
    public class ActiveEffect : GameEventOwner
    {
        //if something is in ActiveEffect, then its owner data (ID, name) cannot be refernced for ANYTHING.  That's okay, right?
        public override GameEventPriority.EventCause GetEventCause()
        {
            return GameEventPriority.EventCause.None;
        }

        public override string GetID() { return ""; }

        public override string GetDisplayName()
        {
            return null;
        }

        [ListCollapse]
        public StateCollection<UniversalState> UniversalStates;
        [ListCollapse]
        public PriorityList<BattleEvent> BeforeTryActions;
        [ListCollapse]
        public PriorityList<BattleEvent> BeforeActions;
        [ListCollapse]
        public PriorityList<BattleEvent> OnActions;
        [ListCollapse]
        public PriorityList<BattleEvent> BeforeExplosions;
        [ListCollapse]
        public PriorityList<BattleEvent> BeforeHits;
        [ListCollapse]
        public PriorityList<BattleEvent> OnHits;
        [ListCollapse]
        public PriorityList<BattleEvent> OnHitTiles;
        [ListCollapse]
        public PriorityList<BattleEvent> AfterActions;
        [ListCollapse]
        public PriorityList<ElementEffectEvent> ElementEffects;

        [ListCollapse]
        public PriorityList<ItemGivenEvent> OnEquips;
        [ListCollapse]
        public PriorityList<ItemGivenEvent> OnPickups;

        [ListCollapse]
        public PriorityList<StatusGivenEvent> BeforeStatusAdds;
        [ListCollapse]
        public PriorityList<StatusGivenEvent> OnStatusAdds;
        [ListCollapse]
        public PriorityList<StatusGivenEvent> OnStatusRemoves;

        [ListCollapse]
        public PriorityList<MapStatusGivenEvent> OnMapStatusAdds;
        [ListCollapse]
        public PriorityList<MapStatusGivenEvent> OnMapStatusRemoves;

        [ListCollapse]
        public PriorityList<SingleCharEvent> OnMapStarts;

        [ListCollapse]
        public PriorityList<SingleCharEvent> OnTurnStarts;
        [ListCollapse]
        public PriorityList<SingleCharEvent> OnTurnEnds;
        [ListCollapse]
        public PriorityList<SingleCharEvent> OnMapTurnEnds;
        [ListCollapse]
        public PriorityList<SingleCharEvent> OnWalks;
        [ListCollapse]
        public PriorityList<SingleCharEvent> OnDeaths;

        [ListCollapse]
        public PriorityList<RefreshEvent> OnRefresh;
        [ListCollapse]
        public PriorityList<RefreshEvent> OnMapRefresh;

        [ListCollapse]
        public PriorityList<HPChangeEvent> ModifyHPs;
        [ListCollapse]
        public PriorityList<HPChangeEvent> RestoreHPs;

        [ListCollapse]
        public PriorityList<BattleEvent> InitActionData;

        public ActiveEffect()
        {
            UniversalStates = new StateCollection<UniversalState>();

            BeforeTryActions = new PriorityList<BattleEvent>();
            BeforeActions = new PriorityList<BattleEvent>();
            OnActions = new PriorityList<BattleEvent>();
            BeforeExplosions = new PriorityList<BattleEvent>();
            BeforeHits = new PriorityList<BattleEvent>();
            OnHits = new PriorityList<BattleEvent>();
            OnHitTiles = new PriorityList<BattleEvent>();
            AfterActions = new PriorityList<BattleEvent>();
            ElementEffects = new PriorityList<ElementEffectEvent>();

            OnEquips = new PriorityList<ItemGivenEvent>();
            OnPickups = new PriorityList<ItemGivenEvent>();

            BeforeStatusAdds = new PriorityList<StatusGivenEvent>();
            OnStatusAdds = new PriorityList<StatusGivenEvent>();
            OnStatusRemoves = new PriorityList<StatusGivenEvent>();
            OnMapStatusAdds = new PriorityList<MapStatusGivenEvent>();
            OnMapStatusRemoves = new PriorityList<MapStatusGivenEvent>();

            OnMapStarts = new PriorityList<SingleCharEvent>();
            OnTurnStarts = new PriorityList<SingleCharEvent>();
            OnTurnEnds = new PriorityList<SingleCharEvent>();
            OnMapTurnEnds = new PriorityList<SingleCharEvent>();
            OnWalks = new PriorityList<SingleCharEvent>();
            OnDeaths = new PriorityList<SingleCharEvent>();

            OnRefresh = new PriorityList<RefreshEvent>();
            OnMapRefresh = new PriorityList<RefreshEvent>();

            ModifyHPs = new PriorityList<HPChangeEvent>();
            RestoreHPs = new PriorityList<HPChangeEvent>();

            InitActionData = new PriorityList<BattleEvent>();
        }

        public void AddOther(ActiveEffect other)
        {
            foreach (UniversalState state in other.UniversalStates)
                UniversalStates.Set(state);

            addOtherPriorityList(BeforeTryActions, other.BeforeTryActions);
            addOtherPriorityList(BeforeActions, other.BeforeActions);
            addOtherPriorityList(OnActions, other.OnActions);
            addOtherPriorityList(BeforeExplosions, other.BeforeExplosions);
            addOtherPriorityList(BeforeHits, other.BeforeHits);
            addOtherPriorityList(OnHits, other.OnHits);
            addOtherPriorityList(OnHitTiles, other.OnHitTiles);
            addOtherPriorityList(AfterActions, other.AfterActions);
            addOtherPriorityList(ElementEffects, other.ElementEffects);

            addOtherPriorityList(OnEquips, other.OnEquips);
            addOtherPriorityList(OnPickups, other.OnPickups);

            addOtherPriorityList(BeforeStatusAdds, other.BeforeStatusAdds);
            addOtherPriorityList(OnStatusAdds, other.OnStatusAdds);
            addOtherPriorityList(OnStatusRemoves, other.OnStatusRemoves);
            addOtherPriorityList(OnMapStatusAdds, other.OnMapStatusAdds);
            addOtherPriorityList(OnMapStatusRemoves, other.OnMapStatusRemoves);

            addOtherPriorityList(OnMapStarts, other.OnMapStarts);
            addOtherPriorityList(OnTurnStarts, other.OnTurnStarts);
            addOtherPriorityList(OnTurnEnds, other.OnTurnEnds);
            addOtherPriorityList(OnMapTurnEnds, other.OnMapTurnEnds);
            addOtherPriorityList(OnWalks, other.OnWalks);
            addOtherPriorityList(OnDeaths, other.OnDeaths);

            addOtherPriorityList(OnRefresh, other.OnRefresh);
            addOtherPriorityList(OnMapRefresh, other.OnMapRefresh);

            addOtherPriorityList(ModifyHPs, other.ModifyHPs);
            addOtherPriorityList(RestoreHPs, other.RestoreHPs);

            addOtherPriorityList(InitActionData, other.InitActionData);
        }

        private void addOtherPriorityList<T>(PriorityList<T> list, PriorityList<T> other)
        {
            foreach (Priority priority in other.GetPriorities())
            {
                foreach (T step in other.GetItems(priority))
                    list.Add(priority, step);
            }
        }
        public int GetTotalCount()
        {
            int total = 0;

            total += BeforeTryActions.Count;
            total += BeforeActions.Count;
            total += OnActions.Count;
            total += BeforeExplosions.Count;
            total += BeforeHits.Count;
            total += OnHits.Count;
            total += OnHitTiles.Count;
            total += AfterActions.Count;
            total += ElementEffects.Count;

            total += OnEquips.Count;
            total += OnPickups.Count;

            total += BeforeStatusAdds.Count;
            total += OnStatusAdds.Count;
            total += OnStatusRemoves.Count;
            total += OnMapStatusAdds.Count;
            total += OnMapStatusRemoves.Count;

            total += OnMapStarts.Count;
            total += OnTurnStarts.Count;
            total += OnTurnEnds.Count;
            total += OnMapTurnEnds.Count;
            total += OnWalks.Count;
            total += OnDeaths.Count;

            total += OnRefresh.Count;
            total += OnMapRefresh.Count;

            total += ModifyHPs.Count;
            total += RestoreHPs.Count;

            total += InitActionData.Count;

            return total;
        }
    }

    [Serializable]
    public class UniversalActiveEffect : ActiveEffect
    {
        public List<ZoneStep> ZoneSteps;
        public UniversalActiveEffect() : base()
        {
            this.ZoneSteps = new List<ZoneStep>();
        }
    }
}
