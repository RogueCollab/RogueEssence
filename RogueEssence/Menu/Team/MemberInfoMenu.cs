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
        int teamSlot;
        bool assembly;
        bool allowAssembly;
        
        public MenuText Title;
        public MenuText PageText;
        public MenuDivider Div;

        public SpeakerPortrait Portrait;
        public MenuText Species;
        public MenuText MetAt;
        public MenuText Category;
        public MenuDivider MainDiv;

        public MenuText Promotions;
        public MenuText[] PromoteMethods;


        public MemberInfoMenu(int teamSlot, bool assembly, bool allowAssembly)
        {
            Bounds = Rect.FromPoints(new Loc(24, 16), new Loc(296, 224));

            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;

            Character player = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot] : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];
            
            MonsterData dexEntry = DataManager.Instance.GetMonster(player.BaseForm.Species);
            BaseMonsterForm formEntry = dexEntry.Forms[player.BaseForm.Form];
            
            int totalLearnsetPages = (int) Math.Ceiling((double) formEntry.LevelSkills.Count / MemberLearnsetMenu.SLOTS_PER_PAGE);
            int totalOtherMemberPages = 3;
            int totalPages = totalLearnsetPages + totalOtherMemberPages;

            Title = new MenuText(Text.FormatKey("MENU_TEAM_INFO"), new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            PageText = new MenuText($"(3/{totalPages})", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Portrait = new SpeakerPortrait(player.BaseForm, new EmoteStyle(0),
                new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);

            string speciesName = dexEntry.GetColoredName();
            if (player.BaseForm.Skin != DataManager.Instance.DefaultSkin)
                speciesName += " (" + DataManager.Instance.GetSkin(player.BaseForm.Skin).GetColoredName() + ")";
            if (player.BaseForm.Gender != Gender.Genderless)
                speciesName += (player.BaseForm.Gender == Gender.Male) ? " (\u2642)" : " (\u2640)";
            else
                speciesName += " (-)";
            Species = new MenuText(Text.FormatKey("MENU_TEAM_SPECIES", speciesName), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 0 + TitledStripMenu.TITLE_OFFSET));
            Category = new MenuText(dexEntry.Title.ToLocal(), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 1 + TitledStripMenu.TITLE_OFFSET));
            if (DataManager.Instance.Save.UUID == player.OriginalUUID)
                MetAt = new MenuText(Text.FormatKey("MENU_TEAM_MET_AT", player.MetAt), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));
            else
                MetAt = new MenuText(Text.FormatKey("MENU_TEAM_TRADED_FROM", player.OriginalTeam), new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));
            MainDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Promotions = new MenuText(Text.FormatKey("MENU_TEAM_PROMOTION"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));
            List<MenuText> validPromotions = new List<MenuText>();

            bool inDungeon = (GameManager.Instance.CurrentScene == DungeonScene.Instance);
            for (int ii = 0; ii < dexEntry.Promotions.Count; ii++)
            {
                if (!DataManager.Instance.DataIndices[DataManager.DataType.Monster].Get(dexEntry.Promotions[ii].Result).Released)
                    continue;
                if (dexEntry.Promotions[ii].IsQualified(player, inDungeon))
                {
                    validPromotions.Add(new MenuText(DataManager.Instance.GetMonster(dexEntry.Promotions[ii].Result).GetColoredName() + ": " + dexEntry.Promotions[ii].GetReqString(),
                        new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (validPromotions.Count + 5) + TitledStripMenu.TITLE_OFFSET)));
                }
                else
                {
                    bool hardReq = false;
                    foreach (PromoteDetail detail in dexEntry.Promotions[ii].Details)
                    {
                        if (detail.IsHardReq() && !detail.GetReq(player))
                        {
                            hardReq = true;
                            break;
                        }
                    }
                    if (!hardReq)
                    {
                        validPromotions.Add(new MenuText(DataManager.Instance.GetMonster(dexEntry.Promotions[ii].Result).GetColoredName() + ": " + dexEntry.Promotions[ii].GetReqString(),
                            new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 8, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (validPromotions.Count + 5) + TitledStripMenu.TITLE_OFFSET), Color.Red));
                    }
                }
            }
            if (validPromotions.Count > 0)
                PromoteMethods = validPromotions.ToArray();
            else
            {
                PromoteMethods = new MenuText[1];
                PromoteMethods[0] = new MenuText(Text.FormatKey("MENU_TEAM_PROMOTE_NONE"), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET));
            }
        }

        public override IEnumerable<IMenuElement> GetElements()
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
            foreach (MenuText method in PromoteMethods)
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
                MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(teamSlot, assembly, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberLearnsetMenu(teamSlot, assembly, allowAssembly, false));
            }
            else if (IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(amtLimit - 1, !assembly, allowAssembly));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(teamSlot - 1, assembly, allowAssembly));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberInfoMenu((teamSlot + DataManager.Instance.Save.ActiveTeam.Players.Count - 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(0, !assembly, allowAssembly));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(teamSlot + 1, assembly, allowAssembly));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberInfoMenu((teamSlot + 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly));
            }
        }
    }
}
