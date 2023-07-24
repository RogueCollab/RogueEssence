using System;
using RogueEssence.Dungeon;
using RogueElements;
using Microsoft.Xna.Framework;

namespace RogueEssence.Data
{
    [Serializable]
    public class TerrainData : IEntryData
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
        /// Special variables that this terrain contains.
        /// They are potentially checked against in a select number of battle events.
        /// </summary>
        public StateCollection<TerrainState> TerrainStates;

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
