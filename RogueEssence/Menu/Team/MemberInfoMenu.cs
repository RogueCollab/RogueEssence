using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MemberInfoMenu : InteractableMenu
    {
    	Team team;
        int teamSlot;
        bool assembly;
        bool allowAssembly;
		bool guest;
        
        public MenuText Title;
        public MenuText PageText;
        public MenuDivider Div;

        public SpeakerPortrait Portrait;
        public MenuText Species;
        public MenuText MetAt;
        public MenuText Category;
        public MenuDivider MainDiv;

        public MenuText Promotions;
        public DialogueText[] PromoteMethods;


        public MemberInfoMenu(Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest) : this(MenuLabel.SUMMARY_MENU_INFO, team, teamSlot, assembly, allowAssembly, guest) { }
        public MemberInfoMenu(string label, Team team, int teamSlot, bool assembly, bool allowAssembly, bool guest)
        {
            Label = label;

            Bounds = Rect.FromPoints(new Loc(24, 16), new Loc(296, 224));
            
            this.team = team;
            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;
			this.guest = guest;

            Character player = null;
			
			if (guest)
			{
				player = team.Guests[teamSlot];
			}
			else
			{
				player = assembly ? ((ExplorerTeam)team).Assembly[teamSlot] : team.Players[teamSlot];
			}
            
            MonsterData dexEntry = DataManager.Instance.GetMonster(player.BaseForm.Species);
            BaseMonsterForm formEntry = dexEntry.Forms[player.BaseForm.Form];
            
            int totalLearnsetPages = (int) Math.Ceiling((double)MemberLearnsetMenu.getEligibleSkills(formEntry) / MemberLearnsetMenu.SLOTS_PER_PAGE);
            int totalOtherMemberPages = 3;
            int totalPages = totalLearnsetPages + totalOtherMemberPages;

            Title = new MenuText(Text.FormatKey("MENU_TEAM_INFO"), new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            PageText = new MenuText($"(3/{totalPages})", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Portrait = new SpeakerPortrait(player.BaseForm, new EmoteStyle(0),
                new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);

            string speciesName = dexEntry.GetColoredName();
            if (DataManager.Instance.GetSkin(player.BaseForm.Skin).Display)
                speciesName += " (" + DataManager.Instance.GetSkin(player.BaseForm.Skin).GetColoredName() + ")";
            if (player.BaseForm.Gender != Gender.Genderless)
                speciesName += (player.BaseForm.Gender == Gender.Male) ? " (\u2642)" : " (\u2640)";
            else
                speciesName += " (-)";
            Species = new MenuText(Text.FormatKey("MENU_TEAM_SPECIES", speciesName), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 0 + TitledStripMenu.TITLE_OFFSET));
            Category = new MenuText(dexEntry.Title.ToLocal(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 1 + TitledStripMenu.TITLE_OFFSET));
            MetAt = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));
			
			if(!guest && !string.IsNullOrWhiteSpace(player.MetAt))
			{
				if (DataManager.Instance.Save.UUID == player.OriginalUUID)
					MetAt.SetText(Text.FormatKey("MENU_TEAM_MET_AT", player.MetAt));
				else
					MetAt.SetText(Text.FormatKey("MENU_TEAM_TRADED_FROM", player.OriginalTeam));
			}

            MainDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Promotions = new MenuText(Text.FormatKey("MENU_TEAM_PROMOTION"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));
            List<DialogueText> validPromotions = new List<DialogueText>();

            int lineCount = 0;
            bool inDungeon = (GameManager.Instance.CurrentScene == DungeonScene.Instance);
            for (int ii = 0; ii < dexEntry.Promotions.Count; ii++)
            {
                DialogueText promoteReqText = new DialogueText(DataManager.Instance.GetMonster(dexEntry.Promotions[ii].Result).GetColoredName() + ": " + dexEntry.Promotions[ii].GetReqString(), new Rect(
                    new Loc(GraphicsManager.MenuBG.TileWidth * 3, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (lineCount + 5) + TitledStripMenu.TITLE_OFFSET),
                    new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, 0)), LINE_HEIGHT);
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(dexEntry.Promotions[ii].Result).Released)
                    continue;
                if (dexEntry.Promotions[ii].IsQualified(player, inDungeon))
                {
                    validPromotions.Add(promoteReqText);
                    lineCount += promoteReqText.GetLineCount();
                }
                else
                {
                    bool hardReq = false;
                    foreach (PromoteDetail detail in dexEntry.Promotions[ii].Details)
                    {
                        if (detail.IsHardReq() && !detail.GetReq(player, inDungeon))
                        {
                            hardReq = true;
                            break;
                        }
                    }
                    if (!hardReq)
                    {
                        promoteReqText.Color = Color.Red;
                        validPromotions.Add(promoteReqText);
                        lineCount += promoteReqText.GetLineCount();
                    }
                }
            }
            if (validPromotions.Count > 0)
                PromoteMethods = validPromotions.ToArray();
            else
            {
                PromoteMethods = new DialogueText[1];
                PromoteMethods[0] = new DialogueText(Text.FormatKey("MENU_TEAM_PROMOTE_NONE"), new Rect(
                    new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET),
                    new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, 0)), LINE_HEIGHT);
            }
        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return Title;
            yield return PageText;
            yield return Div;

            yield return Portrait;
            yield return Species;
            yield return MetAt;
            yield return Category;

            yield return MainDiv;

            yield return Promotions;
            foreach (DialogueText method in PromoteMethods)
                yield return method;
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.ClearMenus();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(team, teamSlot, assembly, allowAssembly, guest));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(team, teamSlot, assembly, allowAssembly, guest, false));
            }
            else if (IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, amtLimit - 1, !assembly, true, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, teamSlot - 1, assembly, true, false));
                }
                else if (guest)
				{
					MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, (teamSlot + team.Guests.Count - 1) % team.Guests.Count, false, false, true));
				}
                else
                    MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, (teamSlot + team.Players.Count - 1) % team.Players.Count, false, false, false));
            }
            else if (IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? ((ExplorerTeam)team).Assembly.Count : team.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, 0, !assembly, true, false));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, teamSlot + 1, assembly, true, false));
                }
                else if (guest)
				{
					MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, (teamSlot + 1) % team.Guests.Count, false, false, true));
				}
                else
                    MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(team, (teamSlot + 1) % team.Players.Count, false, false, false));
            }
        }
    }
}
