using System;
using RogueEssence.Dungeon;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Dev;

namespace RogueEssence.Data
{
    [Serializable]
    public class TerrainData : PassiveData, IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        [Flags]
        public enum Mobility
        {
            Impassable = -1,
            Passable = 0,
            Water = 1,
            Lava = 2,
            Abyss = 4,
            Block = 8,
            All = 15
        }

        public enum TileItemLand
        {
            Normal,
            Fall,
            Destroy
        }

        public enum TileItemDraw
        {
            Normal,
            Transparent,
            Hide
        }

        public enum TileItemAllowance
        {
            Allow,
            Force,
            Forbid
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public EntrySummary GenerateEntrySummary() { return new EntrySummary(Name, Released, Comment); }

        [Dev.DataType(0, DataManager.DataType.Element, false)]
        public int Element;
        public Mobility BlockType;

        public Color MinimapColor;
        public bool BlockDiagonal;
        public bool BlockLight;
        public int ShadowType;

        /// <summary>
        /// What happens when an item lands on this tile.
        /// </summary>
        public TileItemLand ItemLand;

        /// <summary>
        /// How the item's visibility is affected when it's on this tile.
        /// </summary>
        public TileItemDraw ItemDraw;

        /// <summary>
        /// Determines whether items are allowed on this tile, if they have to be forced on, or if they flat out forbid them.
        /// </summary>
        public TileItemAllowance ItemAllow;

        /// <summary>
        /// Special variables that this terrain contains.
        /// They are potentially checked against in a select number of battle events.
        /// </summary>
        [ListCollapse]
        public StateCollection<TerrainState> TerrainStates;

        [ListCollapse]
        public PriorityList<SingleCharEvent> LandedOnTiles;

        public TerrainData()
        {
            Name = new LocalText();
            Comment = "";
            TerrainStates = new StateCollection<TerrainState>();
            LandedOnTiles = new PriorityList<SingleCharEvent>();
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }
}
