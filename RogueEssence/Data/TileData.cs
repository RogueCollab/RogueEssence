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
        public override string ToString()
        {
            return Name.ToLocal();
        }

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
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        public ObjAnimData Anim;
        public Loc Offset;


        /// <summary>
        /// The layer to draw the tile on.  Only supports Bottom, Back and Front for now.
        /// </summary>
        public DrawLayer Layer;
        //public bool BlockLight;

        /// <summary>
        /// Prevents items from landing on it.
        /// </summary>
        public bool BlockItem;
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


        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }
}
