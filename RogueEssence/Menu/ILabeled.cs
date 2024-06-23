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
        public const string TOP_RESCUE = "TOP_RESCUE";
        public const string TOP_CONTINUE = "TOP_CONTINUE";
        public const string TOP_NEW = "TOP_NEW";
        public const string TOP_ROGUE = "TOP_ROGUE";
        public const string TOP_RECORD = "TOP_RECORD";
        public const string TOP_OPTIONS = "TOP_OPTIONS";
        public const string TOP_QUEST = "TOP_QUEST";
        public const string TOP_QUEST_EXIT = "TOP_QUEST_EXIT";
        public const string TOP_MODS = "TOP_MODS";
        public const string TOP_QUIT = "TOP_QUIT";

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
