using System;
using System.Collections.Generic;
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
            Object
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
        /// Allows the tile to be destroyed by certain attacks.
        /// </summary>
        public bool Destructible;

        /// <summary>
        /// Any supereffective move used against this tile will destroy it.  If this is none, any attack will destroy it.  Only used if Destructible is true.
        /// </summary>
        [JsonConverter(typeof(ElementListConverter))]
        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public List<string> EffectiveElements;
        
        /// <summary>
        /// The minimum damage needed to destroy the tile.  Only used if Destructible is true.
        /// </summary>
        public int PowerNeededToDestroy;

        public PriorityList<SingleCharEvent> LandedOnTiles;
        public PriorityList<SingleCharEvent> InteractWithTiles;
        
        /// <summary>
        /// Triggers right before the tile is destroyed.  Only used if Destructible is true.
        /// </summary>
        public PriorityList<SingleCharEvent> OnTileDestroyed;

        public TileData()
        {
            Name = new LocalText();
            Desc = new LocalText();
            Comment = "";
            Anim = new ObjAnimData();
            EffectiveElements = new List<string>();
            LandedOnTiles = new PriorityList<SingleCharEvent>();
            InteractWithTiles = new PriorityList<SingleCharEvent>();
            OnTileDestroyed = new PriorityList<SingleCharEvent>();
        }
        public string GetColoredName()
        {
            return String.Format("[color=#00FF00]{0}[color]", Name.ToLocal());
        }
    }
}
