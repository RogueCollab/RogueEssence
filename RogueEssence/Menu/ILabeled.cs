namespace RogueEssence.Menu
{
    public interface ILabeled
    {
        public string Label { get; }
        public bool HasLabel();
        public bool LabelContains(string substr);
    }

    public abstract class MenuLabel
    {
        //MENU_LABELS
        public const string TOP_MENU = "TOP_MENU";
        public const string MAIN_MENU = "MAIN_MENU";
        public const string INFO_MENU = "INFO_MENU";
        public const string SKILLS_MENU = "SKILLS_MENU";
        public const string INVENTORY_MENU = "INVENTORY_MENU";
        public const string INVENTORY_MENU_REPLACE = "INVENTORY_MENU_REPLACE";
        public const string TACTICS_MENU = "TACTICS_MENU";
        public const string TEAM_MENU = "TEAM_MENU";
        public const string TEAM_MENU_SWITCH = "TEAM_MENU_SWITCH";
        public const string TEAM_MENU_SENDHOME = "TEAM_MENU_SENDHOME";
        public const string GROUND_MENU_ITEM = "GROUND_MENU_ITEM";
        public const string GROUND_MENU_TILE = "GROUND_MENU_TILE";
        public const string OTHERS_MENU = "OTHERS_MENU";
        public const string OPTIONS_MENU = "OPTIONS_MENU";
        public const string SETTINGS_MENU = "SETTINGS_MENU";
        public const string SETTINGS_MENU_PAGE = "SETTINGS_MENU_PAGE";
        public const string KEYBOARD_MENU = "KEYBOARD_MENU";
        public const string GAMEPAD_MENU = "GAMEPAD_MENU";
        public const string BANK_MENU = "BANK_MENU";
        public const string ITEM_AMOUNT_MENU = "ITEM_AMOUNT_MENU";
        public const string SUMMARY_MENU_FEATS = "SUMMARY_MENU_FEATS";
        public const string SUMMARY_MENU_STATS = "SUMMARY_MENU_STATS";
        public const string SUMMARY_MENU_INFO = "SUMMARY_MENU_INFO";
        public const string LEVELUP_MENU = "LEVELUP_MENU";
        public const string RECORDS_MENU = "RECORDS_MENU";
        public const string REPLAY_LIST_MENU = "REPLAY_LIST_MENU";
        public const string REPLAY_CHOSEN_MENU = "REPLAY_CHOSEN_MENU";
        public const string CHOOSE_MONSTER_MENU = "CHOOSE_MONSTER_MENU";
        public const string ROGUE_MENU = "ROGUE_MENU";
        public const string ROGUE_INFO_MENU = "ROGUE_INFO_MENU";
        public const string ROGUE_DEST_MENU = "ROGUE_DEST_MENU";
        public const string ROGUE_SEED_MENU = "ROGUE_SEED_MENU";
        public const string ROGUE_TEAM_NAME_MENU = "ROGUE_TEAM_NAME_MENU";
        public const string ROGUE_CHAR_MENU = "ROGUE_CHAR_MENU";
        public const string ROGUE_CHAR_MENU_DETAILS = "ROGUE_CHAR_MENU_DETAILS";
        public const string NICKNAME_MENU = "NICKNAME_MENU";
        public const string TEAM_NAME_MENU = "TEAM_NAME_MENU";

        //USED IN MULTIPLE MENUS
        public const string TITLE = "TITLE";
        public const string DIV = "DIV";
        public const string MESSAGE = "MESSAGE";
        public const string CURSOR = "CURSOR";

        //TOP MENU OPTIONS
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

        //MAIN MENU OPTIONS
        public const string MAIN_SKILLS = "MAIN_SKILLS";
        public const string MAIN_INVENTORY = "MAIN_INVENTORY";
        public const string MAIN_TACTICS = "MAIN_TACTICS";
        public const string MAIN_TEAM = "MAIN_TEAM";
        public const string MAIN_GROUND = "MAIN_GROUND";
        public const string MAIN_OTHERS = "MAIN_OTHERS";
        public const string MAIN_REST = "MAIN_REST";
        public const string MAIN_SAVE = "MAIN_SAVE";

        //OTHERS MENU OPTIONS
        public const string OTH_MSG_LOG = "OTH_MSG_LOG";
        public const string OTH_SETTINGS = "OTH_SETTINGS";
        public const string OTH_KEYBOARD = "OTH_KEYBOARD";
        public const string OTH_GAMEPAD = "OTH_GAMEPAD";
    }
}
