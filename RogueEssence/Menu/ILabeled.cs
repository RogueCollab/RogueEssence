using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueEssence.Menu
{
    public interface ILabeled
    {
        public string Label { get; }
        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
        public bool LabelContains(string substr)
        {
            return HasLabel() && Label.Contains(substr);
        }
    }

    public enum MenuLabel
    {
        MAIN,
        SKILLS,
        INVENTORY,
        INVENTORY_REPLACE,
        TACTICS,
        TEAM,
        TEAM_SWITCH,
        TEAM_SENDHOME,
        GROUND,
        GROUND_ITEM,
        GROUND_TILE,
        OTHERS
    }
}
