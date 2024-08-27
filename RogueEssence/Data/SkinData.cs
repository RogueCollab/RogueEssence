using System;
using RogueEssence.Content;
using Microsoft.Xna.Framework;

namespace RogueEssence.Data
{
    [Serializable]
    public class SkinData : IEntryData
    {
        public override string ToString()
        {
            return Name.ToLocal();
        }

        public LocalText Name { get; set; }
        public bool Released { get { return true; } }
        [Dev.Multiline(0)]
        public string Comment { get; set; }

        public int IndexNum;
        
        /// <summary>
        /// The symbol displayed next the characters species name.
        /// </summary>
        public char Symbol;
        
        /// <summary>
        /// The color displayed on the minimap.
        /// </summary>
        public Color MinimapColor;
        
        /// <summary>
        /// The VFX effect played when becoming the team leader.
        /// </summary>
        public BattleFX LeaderFX;
        
        /// <summary>
        /// Whether to show the skin type in the member info menu.
        /// </summary>
        public bool Display;
        
        /// <summary>
        /// Whether the character with this skin can be sent home during Roguelocke.
        /// </summary>
        public bool Challenge;

        public EntrySummary GenerateEntrySummary()
        {
            return new EntrySummary(Name, Released, Comment, IndexNum);
        }

        public SkinData()
        {
            Name = new LocalText();
            Comment = "";
        }

        public SkinData(LocalText name, char symbol)
        {
            Name = name;
            Comment = "";
            Symbol = symbol;
            LeaderFX  = new BattleFX();
        }

        public string GetColoredName()
        {
            return String.Format("{0}", Name.ToLocal());
        }
    }
}
