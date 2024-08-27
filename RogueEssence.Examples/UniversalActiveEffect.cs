using System;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Examples
{
    public class UniversalActiveEffect : UniversalBaseEffect
    { 
        public UniversalActiveEffect() { }
        
        public override int GetRange(Character character, ref SkillData entry)
        {
            int rangeMod = 0;
            
            return rangeMod;
        }
    }
}
