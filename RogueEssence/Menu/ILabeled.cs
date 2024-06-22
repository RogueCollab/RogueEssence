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
    public struct LabeledElementIndex 
    {
        public List<ILabeled> List { get; private set; } = null;
        public int Index { get; private set; } = -1;
        public LabeledElementIndex(List<ILabeled> list, int index)
        {
            List = list;
            Index = index;
        }
        public readonly bool Found() => List!=null && Index>=0 && Index<=List.Count;
    }

    public abstract class MenuLabel
    {
        public const string MAIN = "MAIN";
        public const string SKILLS = "SKILLS";
        public const string INVENTORY = "INVENTORY";
        public const string INVENTORY_REPLACE = "INVENTORY_REPLACE";
        public const string TACTICS = "TACTICS";
        public const string TEAM = "TEAM";
        public const string TEAM_SWITCH = "TEAM_SWITCH";
        public const string TEAM_SENDHOME = "TEAM_SENDHOME";
        public const string GROUND = "GROUND";
        public const string GROUND_ITEM = "GROUND_ITEM";
        public const string GROUND_TILE = "GROUND_TILE";
        public const string OTHERS = "OTHERS";
        public const string REST = "REST";
        public const string SAVE = "SAVE";
        public const string MSG_LOG = "MSG_LOG";
        public const string SETTINGS = "SETTINGS";
        public const string KEYBOARD = "KEYBOARD";
        public const string GAMEPAD = "GAMEPAD";

        public const string CURSOR = "cursor";
        //INFO MENU
        public const string INFO_MENU = "INFO_MENU";
    }
}
