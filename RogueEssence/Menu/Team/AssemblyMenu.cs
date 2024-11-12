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
        public enum AssemblySortMode
        {
            Recent,
            Level,
            Species,
            Nickname
        }

        public delegate void OnChooseTeam(List<int> slot);
        private const int SLOTS_PER_PAGE = 6;

        SpeakerPortrait portrait;
        TeamMiniSummary summaryMenu;
        
        private Action teamChanged;

        private List<int> assemblyView;
        private AssemblySortMode sortMode;

        public AssemblyMenu(int defaultChoice, Action teamChanged, AssemblySortMode sort = AssemblySortMode.Recent)
        {
            int menuWidth = 160;
            this.teamChanged = teamChanged;
            sortMode = sort;

            List<MenuChoice> flatChoices = new List<MenuChoice>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Players.Count; ii++)
            {
                int index = ii;
                Character character = DataManager.Instance.Save.ActiveTeam.Players[index];
                bool enabled = !ChoosingLeader(ii);
                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), enabled ? Color.Lime : TextIndigo);
                MenuText memberLvLabel = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"), new Loc(menuWidth - 8 * 7 + 6, 1),
                    DirV.Up, DirH.Right, enabled ? Color.Lime : TextIndigo);
                MenuText memberLv = new MenuText(character.Level.ToString(), new Loc(menuWidth - 8 * 7 + 6 + GraphicsManager.TextFont.SubstringWidth(DataManager.Instance.Start.MaxLevel.ToString()), 1), 
                    DirV.Up, DirH.Right, enabled ? Color.Lime : TextIndigo);
                flatChoices.Add(new MenuElementChoice(() => { Choose(index, false); }, true, memberName, memberLvLabel, memberLv));
            }

            assemblyView = getSortedAssembly();
            for (int ii = 0; ii < assemblyView.Count; ii++)
            {
                int index = ii;
                Character character = DataManager.Instance.Save.ActiveTeam.Assembly[assemblyView[index]];
                Color color = CanChooseAssembly(ii) ? (character.IsFavorite ? Color.Yellow : Color.White) : Color.Red;
                MenuText memberName = new MenuText(character.BaseName, new Loc(2, 1), color);
                MenuText memberLvLabel = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"), new Loc(menuWidth - 8 * 7 + 6, 1),
                    DirV.Up, DirH.Right, color);
                MenuText memberLv = new MenuText(character.Level.ToString(), new Loc(menuWidth - 8 * 7 + 6 + GraphicsManager.TextFont.SubstringWidth(DataManager.Instance.Start.MaxLevel.ToString()), 1), DirV.Up, DirH.Right, color);

                int assemblyIndex = assemblyView[index];
                flatChoices.Add(new MenuElementChoice(() => { Choose(assemblyIndex, true); }, true, memberName, memberLvLabel, memberLv));
            }
            IChoosable[][] box = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);
            defaultChoice = Math.Min(defaultChoice, flatChoices.Count - 1);
            int startChoice = defaultChoice % SLOTS_PER_PAGE;
            int startPage = defaultChoice / SLOTS_PER_PAGE;

            summaryMenu = new TeamMiniSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - VERT_SPACE * 5),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            portrait = new SpeakerPortrait(MonsterID.Invalid, new EmoteStyle(0), new Loc(GraphicsManager.ScreenWidth - 24 - 40, 16), true);

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_ASSEMBLY_TITLE"), box, startChoice, startPage, SLOTS_PER_PAGE);

        }

        private List<int> getSortedAssembly()
        {
            List<int> sortedAssembly = new List<int>();
            for (int ii = 0; ii < DataManager.Instance.Save.ActiveTeam.Assembly.Count; ii++)
                sortedAssembly.Add(ii);
            sortedAssembly.Sort(assemblyCompare);
            return sortedAssembly;
        }

        public int assemblyCompare(int key1, int key2)
        {
            CharData data1 = DataManager.Instance.Save.ActiveTeam.Assembly[key1];
            CharData data2 = DataManager.Instance.Save.ActiveTeam.Assembly[key2];
            if (data1.IsFavorite != data2.IsFavorite)
            {
                if (data1.IsFavorite)
                    return -1;
                else
                    return 1;
            }

            switch (sortMode)
            {
                case AssemblySortMode.Level:
                    return Math.Sign(-1 * (data1.Level - data2.Level));
                case AssemblySortMode.Nickname:
                    return String.Compare(data1.BaseName, data2.BaseName);
                case AssemblySortMode.Species:
                    {
                        int dex1 = DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(data1.BaseForm.Species).SortOrder;
                        int dex2 = DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(data2.BaseForm.Species).SortOrder;
                        return Math.Sign(dex1 - dex2);
                    }
            }

            return Math.Sign(key1 - key2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">Represents the true index, not the one on display</param>
        /// <param name="assembly"></param>
        public void Choose(int index, bool assembly)
        {
            MenuManager.Instance.AddMenu(new AssemblyChosenMenu(index, assembly, this), true);
        }

        public bool ChoosingLeader(int choice)
        {
            return choice == DataManager.Instance.Save.ActiveTeam.LeaderIndex;
        }

        public bool ChoosingStuckMember(int choice)
        {
            if (DataManager.Instance.Save.ActiveTeam.Players[choice].IsPartner)
                return true;
            if (DataManager.Instance.Save is RogueProgress && DataManager.Instance.GetSkin(DataManager.Instance.Save.ActiveTeam.Players[choice].BaseForm.Skin).Challenge && !DataManager.Instance.Save.ActiveTeam.Players[choice].Dead)
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choice">Choice must be premapped.</param>
        /// <returns></returns>
        public bool CanChooseAssembly(int choice)
        {
            Character character = DataManager.Instance.Save.ActiveTeam.Assembly[choice];
            return !character.Dead && (DataManager.Instance.Save.ActiveTeam.Players.Count < ExplorerTeam.MAX_TEAM_SLOTS);
        }

        public void ChooseLeader(int choice)
        {
            DataManager.Instance.Save.ActiveTeam.LeaderIndex = choice;
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentChoiceTotal, teamChanged, sortMode));
            teamChanged();
        }

        public void ChooseTeam(int choice)
        {
            GroundScene.Instance.SilentSendHome(choice);
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentChoiceTotal, teamChanged, sortMode));
            teamChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choice">Choice must be pre-mapped</param>
        public void ChooseAssembly(int choice)
        {
            GroundScene.Instance.SilentAddToTeam(choice);
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentChoiceTotal, teamChanged, sortMode));
            teamChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choice">Choice must be premapped</param>
        public void ReleaseAssembly(int choice)
        {
            DataManager.Instance.Save.ActiveTeam.Assembly.RemoveAt(choice);
            assemblyView.RemoveAt(choice);
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentChoiceTotal, teamChanged, sortMode));
        }
        public void ConfirmRename(string name)
        {
            MenuManager.Instance.RemoveMenu();
            int currentChoice = CurrentChoiceTotal;
            if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
            {
                DataManager.Instance.Save.ActiveTeam.Players[currentChoice].Nickname = name;
                teamChanged();
            }
            else
            {
                int assemblyIndex = currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count;
                DataManager.Instance.Save.ActiveTeam.Assembly[assemblyView[assemblyIndex]].Nickname = name;
            }
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentChoiceTotal, teamChanged, sortMode));
        }
        public void ToggleFave()
        {
            MenuManager.Instance.RemoveMenu();
            int currentChoice = CurrentChoiceTotal;
            if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
                DataManager.Instance.Save.ActiveTeam.Players[currentChoice].IsFavorite = !DataManager.Instance.Save.ActiveTeam.Players[currentChoice].IsFavorite;
            else
            {
                int assemblyIndex = currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count;
                Character chara = DataManager.Instance.Save.ActiveTeam.Assembly[assemblyView[assemblyIndex]];
                chara.IsFavorite = !chara.IsFavorite;
            }
            MenuManager.Instance.ReplaceMenu(new AssemblyMenu(CurrentChoiceTotal, teamChanged, sortMode));
        }

        protected override void UpdateKeys(InputManager input)
        {
            //however, when switching between members, the settings are kept even if invalid for new members, just display legal substitutes in those cases
            if (input.JustPressed(FrameInput.InputType.SortItems))
            {
                GameManager.Instance.SE("Menu/Sort");
                MenuManager.Instance.ReplaceMenu(new AssemblyMenu(DataManager.Instance.Save.ActiveTeam.Players.Count, teamChanged, (AssemblySortMode)(((int)sortMode + 1) % 4)));
            }
            else if (input.JustPressed(FrameInput.InputType.SelectItems))
            {
                int currentChoice = CurrentChoiceTotal;
                //instantly put the team on or remove it
                if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
                {
                    if (!ChoosingLeader(currentChoice) && !ChoosingStuckMember(currentChoice))
                    {
                        GameManager.Instance.SE("Menu/Toggle");
                        GroundScene.Instance.SilentSendHome(currentChoice);
                        teamChanged();
                        MenuManager.Instance.ReplaceMenu(new AssemblyMenu(currentChoice, teamChanged, sortMode));
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else
                {
                    int assemblyIndex = currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (CanChooseAssembly(assemblyView[assemblyIndex]))
                    {
                        GameManager.Instance.SE("Menu/Toggle");
                        GroundScene.Instance.SilentAddToTeam(assemblyView[assemblyIndex]);
                        teamChanged();
                        MenuManager.Instance.ReplaceMenu(new AssemblyMenu(currentChoice, teamChanged, sortMode));
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
            int currentChoice = CurrentChoiceTotal;
            Character subjectChar = null;
            if (currentChoice < DataManager.Instance.Save.ActiveTeam.Players.Count)
                subjectChar = DataManager.Instance.Save.ActiveTeam.Players[currentChoice];
            else
            {
                int assemblyIndex = currentChoice - DataManager.Instance.Save.ActiveTeam.Players.Count;
                subjectChar = DataManager.Instance.Save.ActiveTeam.Assembly[assemblyView[assemblyIndex]];
            }
                
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
