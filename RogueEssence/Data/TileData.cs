using System;
using RogueEssence.Dungeon;
using RogueEssence.Dev;
using RogueEssence.Content;
using RogueElements;
using Newtonsoft.Json;
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
            Unlockable,
            Destructible
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

        /// <summary>
        /// Element for the tile- anything supereffective against this type will destroy it.  If this is none, any attack will destroy it.  Only used if TriggerType is Destructible.
        /// </summary>
        [JsonConverter(typeof(ElementConverter))]
        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public string TileElement;
        
        /// <summary>
        /// If true, only attacks of TileElement will destroy the tile, rather than supereffective attacks.  Only used if TriggerType is Destructible.
        /// </summary>
        public bool SpecificElementDestroysTile;

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
