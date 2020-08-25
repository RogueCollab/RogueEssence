using System;
using RogueEssence.Dungeon;
using RogueEssence.Content;
using RogueElements;
using Microsoft.Xna.Framework;

namespace RogueEssence.Data
{
    [Serializable]
    public class TileData : IDescribedData
    {
        public enum TriggerType
        {
            None,
            Site,
            Passage,
            Trap,
            Switch,
            Blocker,
            Unlockable
        }

        public LocalText Name { get; set; }

        [Dev.Multiline(0)]
        public LocalText Desc { get; set; }
        public bool Released { get; set; }
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public ObjAnimData Anim;
        public bool ObjectLayer;
        //public bool BlockLight;
        public TriggerType StepType;
        public Loc MinimapIcon;
        public Color MinimapColor;

        public PriorityList<SingleCharEvent> LandedOnTiles;
        public PriorityList<SingleCharEvent> InteractWithTiles;

        public TileData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
            Anim = new ObjAnimData();
            LandedOnTiles = new PriorityList<SingleCharEvent>();
            InteractWithTiles = new PriorityList<SingleCharEvent>();
        }
    }
}
