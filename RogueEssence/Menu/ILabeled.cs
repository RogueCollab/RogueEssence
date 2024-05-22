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
    }

    public enum MenuLabel
    {
        MAIN,
        SKILLS,
        INVENTORY,
        INVENTORY_REPLACE,
    }
}
