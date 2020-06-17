using System;
using RogueEssence.Dungeon;
using RogueElements;

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

        public override string GetName()
        {
            return null;
        }

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
        
        public PriorityList<HPChangeEvent> ModifyHPs;
        public PriorityList<HPChangeEvent> RestoreHPs;

        public PriorityList<BattleEvent> InitActionData;

        public ActiveEffect()
        {
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

            ModifyHPs = new PriorityList<HPChangeEvent>();
            RestoreHPs = new PriorityList<HPChangeEvent>();

            InitActionData = new PriorityList<BattleEvent>();
        }

    }
}
