using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class TacticsMenu : BaseSettingsMenu
    {
        //needs a summary menu
        private int releasedTactics;

        public TacticsMenu()
        {
            //individual tactics
            MenuSetting[] totalChoices = new MenuSetting[DataManager.Instance.Save.ActiveTeam.Players.Count + 1];
            
            for (int ii = DataManager.Instance.Save.ActiveTeam.Players.Count-1; ii >= 0; ii--)
            {
                Character character = DataManager.Instance.Save.ActiveTeam.Players[ii];
                List<string> choices = new List<string>();
                int tacticIndex = -1;
                for (int jj = 0; jj < DataManager.Instance.DataIndices[DataManager.DataType.AI].Count; jj++)
                {
                    AIEntrySummary summary = DataManager.Instance.DataIndices[DataManager.DataType.AI].Entries[jj] as AIEntrySummary;
                    if (summary.Assignable)
                    {
                        if (jj == character.Tactic.ID)
                            tacticIndex = jj;
                        choices.Add(DataManager.Instance.DataIndices[DataManager.DataType.AI].Entries[jj].GetColoredName());
                    }
                }
                releasedTactics = choices.Count;
                if (tacticIndex == -1)
                {
                    tacticIndex = releasedTactics;
                    choices.Add("---");
                }

                totalChoices[ii + 1] = new MenuSetting(character.GetDisplayName(true), character.Dead ? Color.Red : Color.White, character.Dead ? Color.DarkRed : Color.Yellow, 88, 72, choices, tacticIndex, tacticIndex, confirmAction);
            }

            //tactics meeting
            List<string> allChoices = new List<string>();
            for (int jj = 0; jj < DataManager.Instance.DataIndices[DataManager.DataType.AI].Count; jj++)
            {
                if (DataManager.Instance.DataIndices[DataManager.DataType.AI].Entries[jj].Released)
                    allChoices.Add(DataManager.Instance.DataIndices[DataManager.DataType.AI].Entries[jj].GetColoredName());
            }

            int groupTactic = totalChoices[1].CurrentChoice;
            for (int ii = 2; ii < totalChoices.Length; ii++)
            {
                if (groupTactic != totalChoices[ii].CurrentChoice)
                {
                    groupTactic = -1;
                    break;
                }
            }
            if (groupTactic == -1)
            {
                groupTactic = releasedTactics;
                allChoices.Add("---");
            }
            totalChoices[0] = new MenuSetting(Text.FormatKey("MENU_TACTICS_EVERYONE"), 88, 72, allChoices, groupTactic, confirmAction);

            Initialize(new Loc(16, 16), 224, Text.FormatKey("MENU_TACTICS_TITLE"), totalChoices);
        }


        protected override void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.TacticMenu))
                MenuManager.Instance.ClearMenus();
            else
                base.UpdateKeys(input);
        }

        private void confirmAction()
        {
            //MenuManager.Instance.RemoveMenu();
            SaveCurrentChoices();

            GameAction cmd = new GameAction(GameAction.ActionType.Tactics, Dir8.None);
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
                cmd.AddArg(TotalChoices[ii + 1].CurrentChoice);
            MenuManager.Instance.NextAction = (GameManager.Instance.CurrentScene == DungeonScene.Instance) ? DungeonScene.Instance.ProcessPlayerInput(cmd) : GroundScene.Instance.ProcessInput(cmd);
        }

        protected override void SettingChanged(int index)
        {
            //if the setting was changed from the "---" index, then erase it
            if (TotalChoices[index].CurrentChoice < releasedTactics && TotalChoices[index].TotalChoices.Count > releasedTactics)
                TotalChoices[index].TotalChoices.RemoveAt(releasedTactics);

            if (index == 0)
            {
                if (TotalChoices[0].CurrentChoice < releasedTactics)
                {
                    //if the meeting setting was changed, change all the other choices
                    for (int ii = 1; ii < TotalChoices.Length; ii++)
                        TotalChoices[ii].SetChoice(TotalChoices[0].CurrentChoice);

                    for (int ii = 1; ii < TotalChoices.Length; ii++)
                        SettingChanged(ii);
                }
            }
            else
            {
                //otherwise, if an individual setting was changed, check to update the meeting setting
                int groupTactic = TotalChoices[1].CurrentChoice;
                for (int ii = 2; ii < TotalChoices.Length; ii++)
                {
                    if (groupTactic != TotalChoices[ii].CurrentChoice)
                    {
                        groupTactic = releasedTactics;
                        break;
                    }
                }
                //before setting, ensure the --- exists
                if (groupTactic != TotalChoices[0].CurrentChoice)
                {
                    if (groupTactic == releasedTactics && TotalChoices[0].TotalChoices.Count == releasedTactics)
                        TotalChoices[0].TotalChoices.Add("---");

                    SetCurrentSetting(0, groupTactic);
                }
            }

            base.SettingChanged(index);
        }


        protected override void ChoiceChanged()
        {
            //change summary menu description

            base.ChoiceChanged();
        }
    }
}
