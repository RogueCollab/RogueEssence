using System;
using RogueEssence.Dungeon;
using RogueElements;
using System.Runtime.Serialization;

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

        public override int GetID() { return -1; }

        public override string GetDisplayName()
        {
            return null;
        }

        public StateCollection<UniversalState> UniversalStates;
        public PriorityList<BattleEvent> BeforeTryActions;
        public PriorityList<BattleEvent> BeforeActions;
        public PriorityList<BattleEvent> OnActions;
        public PriorityList<BattleEvent> BeforeExplosions;
        public PriorityList<BattleEvent> BeforeHits;
        public PriorityList<BattleEvent> OnHits;
        public PriorityList<BattleEvent> OnHitTiles;
        public PriorityList<BattleEvent> AfterActions;
        public PriorityList<ElementEffectEvent> ElementEffects;

        public PriorityList<StatusGivenEvent> BeforeStatusAdds;
        public PriorityList<StatusGivenEvent> OnStatusAdds;
        public PriorityList<StatusGivenEvent> OnStatusRemoves;

        public PriorityList<MapStatusGivenEvent> OnMapStatusAdds;
        public PriorityList<MapStatusGivenEvent> OnMapStatusRemoves;

        public PriorityList<SingleCharEvent> OnMapStarts;

        public PriorityList<SingleCharEvent> OnTurnStarts;
        public PriorityList<SingleCharEvent> OnTurnEnds;
        public PriorityList<SingleCharEvent> OnMapTurnEnds;
        public PriorityList<SingleCharEvent> OnWalks;
        public PriorityList<SingleCharEvent> OnDeaths;

        public PriorityList<RefreshEvent> OnRefresh;
        public PriorityList<RefreshEvent> OnMapRefresh;

        public PriorityList<HPChangeEvent> ModifyHPs;
        public PriorityList<HPChangeEvent> RestoreHPs;

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
    }
}
