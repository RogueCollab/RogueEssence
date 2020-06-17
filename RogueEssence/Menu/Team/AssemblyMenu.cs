using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using RogueEssence.Ground;

namespace RogueEssence.Menu
{
    public class AssemblyMenu : MultiPageMenu
    {
        public delegate void OnChooseTeam(List<int> slot);
        private const int SLOTS_PER_PAGE = 6;

        SpeakerPortrait portrait;
        TeamMiniSummary summaryMenu;
        
        private Action teamChanged;

        public AssemblyMenu(int defaultChoice, Action teamChanged)
        {
            int menuWidth = 152;
            this.teamChanged = teamChanged;

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                int index = ii;
                Character character = DataManager.Instance.Save.ActiveTeam.Players[index];
                bool enabled = !ChoosingLeader(ii);
                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), enabled ? Color.Lime : TextIndigo);
                MenuText memberLv = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level), new Loc(menuWidth - 8 * 4, 1),
                    DirV.Up, DirH.Right, enabled ? Color.Lime : TextIndigo);
                flatChoices.Add(new MenuElementChoice(() => { Choose(index, false); }, true, memberName, memberLv));
            }
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Assembly.Count; ii++)
            {
                int index = ii;
                Character character = DataManager.Instance.Save.ActiveTeam.Assembly[index];
                Color color = CanChooseAssembly(ii) ? (character.IsFavorite ? Color.Yellow : Color.White) : Color.Red;
                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), color);
                MenuText memberLv = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level), new Loc(menuWidth - 8 * 4, 1),
                    DirV.Up, DirH.Right, color);
                flatChoices.Add(new MenuElementChoice(() => { Choose(index, true); }, true, memberName, memberLv));
            }
            List<MenuChoice[]> box = SortIntoPages(flatChoices, SLOTS_PER_PAGE);
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;

            summaryMenu = new TeamMiniSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 5),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            portrait = new SpeakerPortrait(new MonsterID(), new EmoteStyle(0), new Loc(GraphicsManager.ScreenWidth - 32 - 40, 16), true);

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_ASSEMBLY_TITLE"), box.ToArray(), startChoice, startPage, SLOTS_PER_PAGE);

        }

        public void Choose(int index, bool assembly)
        {
            MenuManager.Instance.AddMenu(new AssemblyChosenMenu(index, assembly, this), true);
        }

        public bool ChoosingLeader(int choice)
        {
            return choice == DataManager.Instance.Save.ActiveTeam.LeaderIndex;
        }

        public bool CanChooseAssembly(int choice)
        {
            Character character = DataManager.Instance.Save.ActiveTeam.Assembly[choice];
            return !character.Dead && (DataManager.Instance.Save.ActiveTeam.Players.Count < ExplorerTeam.MAX_TEAM_SLOTS);
        }

        public void ChooseLeader(int choice)
        {
            DataManager.Instance.Save.ActiveTeam.LeaderIndex = choice;
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentPage * SpacesPerPage + CurrentChoice, teamChanged));
            teamChanged();
        }

        public void ChooseTeam(int choice)
        {
            GroundScene.Instance.SilentSendHome(choice);
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentPage * SpacesPerPage + CurrentChoice, teamChanged));
            teamChanged();
        }
        public void ChooseAssembly(int choice)
        {
            GroundScene.Instance.SilentAddToTeam(choice);
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentPage * SpacesPerPage + CurrentChoice, teamChanged));
            teamChanged();
        }
        public void ReleaseAssembly(int choice)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(choice);
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentPage * SpacesPerPage + CurrentChoice, teamChanged));
        }
        public void ConfirmRename(string name)
        {
            MenuManager.Instance.RemoveMenu();
            int currentChoice = CurrentPage * SpacesPerPage + CurrentChoice;
            if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
            {
                DataManager.Instance.Save.ActiveTeam.Players[currentChoice].Nickname = name;
                teamChanged();
            }
            else
                DataManager.Instance.Save.ActiveTeam.Assembly[currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count].Nickname = name;
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentPage * SpacesPerPage + CurrentChoice, teamChanged));
        }
        public void ToggleFave()
        {
            MenuManager.Instance.RemoveMenu();
            int currentChoice = CurrentPage * SpacesPerPage + CurrentChoice;
            if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
                DataManager.Instance.Save.ActiveTeam.Players[currentChoice].IsFavorite = !DataManager.Instance.Save.ActiveTeam.Players[currentChoice].IsFavorite;
            else
            {
                Character chara = DataManager.Instance.Save.ActiveTeam.Assembly[currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count];
                chara.IsFavorite = !chara.IsFavorite;
                DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count);
                DataManager.Instance.Save.ActiveTeam.Assembly.Insert(0, chara);
            }
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(DataManager.Instance.Save.ActiveTeam.Players.Count, teamChanged));
        }

        protected override void UpdateKeys(InputManager input)
        {
            //however, when switching between members, the settings are kept even if invalid for new members, just display legal substitutes in those cases
            if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Sort");
                //TODO: make it sort the team based on index or alphabet
            }
            else if (input.JustPressed(FrameInput.InputType.SelectItems))
            {
                int currentChoice = CurrentPage * SpacesPerPage + CurrentChoice;
                //instantly put the team on or remove it
                if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
                {
                    if (!ChoosingLeader(currentChoice))
                    {
                        GameManager.Instance.SE("Menu/Toggle");
                        GroundScene.Instance.SilentSendHome(currentChoice);
                        teamChanged();
                        MenuManager.Instance.ReplaceMenu(new AssemblyMenu(currentChoice, teamChanged));
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else
                {
                    if (CanChooseAssembly(currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count))
                    {
                        GameManager.Instance.SE("Menu/Toggle");
                        GroundScene.Instance.SilentAddToTeam(currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count);
                        teamChanged();
                        MenuManager.Instance.ReplaceMenu(new AssemblyMenu(currentChoice, teamChanged));
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
            }
            else
                base.UpdateKeys(input);
        }

        protected override void ChoiceChanged()
        {
            int currentChoice = CurrentPage * SpacesPerPage + CurrentChoice;
            Character subjectChar = null;
            if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
                subjectChar = DataManager.Instance.Save.ActiveTeam.Players[currentChoice];
            else
                subjectChar = DataManager.Instance.Save.ActiveTeam.Assembly[currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count];
                
            summaryMenu.SetMember(subjectChar);

            portrait.Speaker = subjectChar.BaseForm;
                
            base.ChoiceChanged();
        }

        protected override void Canceled()
        {
            base.Canceled();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            portrait.Draw(spriteBatch, new Loc());

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }

        //public int[] GetTeamMappings()
        //{
        //    //script will take this array, and make an array of bools in script of the current team
        //    //iterate the returned array and mark lua array's index at each > -1 index as true
        //    //if there is any false index, must fade
        //    //if there is any -1 in the originating array, must fade
        //    //keep track of
        //}

        //public bool GetNicknameChanged()
        //{
        //    return nicknameChanged;
        //}

        //public bool GetLeaderChanged()
        //{
        //    return oldLeader == DataManager.Instance.Save.ActiveTeam.Leader;
        //}

    }
}
