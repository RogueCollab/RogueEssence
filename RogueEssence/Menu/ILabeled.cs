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
        //MULTIPLE MENUS
        public const string TITLE = "TITLE";
        public const string DIV = "DIV";
        public const string MESSAGE = "MESSAGE";
        public const string CURSOR = "CURSOR";

        //TOP MENU
        public const string TOP_MENU = "TOP_MENU";
        public const string TOP_TITLE_SUMMARY = "TOP_TITLE_SUMMARY";
        //MAIN MENU
        public const string MAIN_MENU = "MAIN_MENU";
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
        //OTHERS MENU
        public const string OTHERS_MENU = "OTHERS_MENU";
        public const string MSG_LOG = "MSG_LOG";
        public const string SETTINGS = "SETTINGS";
        public const string KEYBOARD = "KEYBOARD";
        public const string GAMEPAD = "GAMEPAD";
        //INFO MENU
        public const string INFO_MENU = "INFO_MENU";
    }
}
