using System;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.Data
{
    [Serializable]
    public class PassiveData : Dev.EditorData
    {

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

        public PriorityList<BattleEvent> BeforeTryActions;
        public PriorityList<BattleEvent> BeforeActions;
        public PriorityList<BattleEvent> OnActions;
        public PriorityList<BattleEvent> BeforeHittings;
        public PriorityList<BattleEvent> BeforeBeingHits;
        public PriorityList<BattleEvent> AfterHittings;
        public PriorityList<BattleEvent> AfterBeingHits;
        public PriorityList<BattleEvent> OnHitTiles;
        public PriorityList<BattleEvent> AfterActions;

        public PriorityList<ElementEffectEvent> UserElementEffects;
        public PriorityList<ElementEffectEvent> TargetElementEffects;
        public PriorityList<HPChangeEvent> ModifyHPs;
        public PriorityList<HPChangeEvent> RestoreHPs;

        public PassiveData()
        {
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

            BeforeTryActions = new PriorityList<BattleEvent>();
            BeforeActions = new PriorityList<BattleEvent>();
            OnActions = new PriorityList<BattleEvent>();
            BeforeHittings = new PriorityList<BattleEvent>();
            BeforeBeingHits = new PriorityList<BattleEvent>();
            AfterHittings = new PriorityList<BattleEvent>();
            AfterBeingHits = new PriorityList<BattleEvent>();
            OnHitTiles = new PriorityList<BattleEvent>();
            AfterActions = new PriorityList<BattleEvent>();

            UserElementEffects = new PriorityList<ElementEffectEvent>();
            TargetElementEffects = new PriorityList<ElementEffectEvent>();
            ModifyHPs = new PriorityList<HPChangeEvent>();
            RestoreHPs = new PriorityList<HPChangeEvent>();
        }
    }


    [Serializable]
    public class ProximityData : PassiveData
    {
        public int Radius;
        public Alignment TargetAlignments;

        //TODO: IMPORTANT: OnEnters and OnLeaves DO NOT WORK at this time
        //also DO NOT ADD ANYTHING TO A PROXIMITY PASSIVE'S ONREFRESH
        public PriorityList<SingleCharEvent> OnEnters;
        public PriorityList<SingleCharEvent> OnLeaves;

        public PriorityList<BattleEvent> BeforeExplosions;


        public ProximityData()
        {
            Radius = -1;

            OnEnters = new PriorityList<SingleCharEvent>();
            OnLeaves = new PriorityList<SingleCharEvent>();

            BeforeExplosions = new PriorityList<BattleEvent>();
        }
    }


    [Serializable]
    public class ProximityPassive : PassiveData
    {
        public ProximityData ProximityEvent;

        public ProximityPassive()
        {
            ProximityEvent = new ProximityData();
        }
    }
}
