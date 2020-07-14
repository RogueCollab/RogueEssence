using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class MemberFeaturesMenu : InteractableMenu
    {
        int teamSlot;
        bool assembly;
        bool allowAssembly;
        public MenuText Title;
        public MenuDivider Div;

        public SpeakerPortrait Portrait;
        public MenuText Name;
        public MenuText Level;
        public MenuText HP;
        public MenuText Fullness;

        public MenuText Elements;

        public MenuDivider MainDiv;
        public MenuText SkillTitle;
        public MenuText[] Skills;

        public MenuDivider IntrinsicDiv;
        public MenuText Intrinsic;
        public DialogueText IntrinsicDesc;

        public MemberFeaturesMenu(int teamSlot, bool assembly, bool allowAssembly)
        {
            Bounds = Rect.FromPoints(new Loc(24, 16), new Loc(296, 224));

            this.teamSlot = teamSlot;
            this.assembly = assembly;
            this.allowAssembly = allowAssembly;

            Character player = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly[teamSlot] : DataManager.Instance.Save.ActiveTeam.Players[teamSlot];

            //TODO: align this text properly
            Title = new MenuText(Text.FormatKey("MENU_TEAM_FEATURES") + " (1/3)", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            Portrait = new SpeakerPortrait(player.BaseForm, new EmoteStyle(0),
                Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET), false);
            string speciesText = player.BaseName + " / " + player.FullFormName;
            Name = new MenuText(speciesText, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET));

            ElementData element1 = DataManager.Instance.GetElement(player.Element1);
            ElementData element2 = DataManager.Instance.GetElement(player.Element2);

            string typeString = String.Format("{0}\u2060{1}", element1.Symbol, element1.Name.ToLocal());
            if (player.Element2 != 00)
                typeString += "/" + String.Format("{0}\u2060{1}", element2.Symbol, element2.Name.ToLocal());
            bool origElements = (player.Element1 == DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form].Element1);
            origElements &= (player.Element2 == DataManager.Instance.GetMonster(player.BaseForm.Species).Forms[player.BaseForm.Form].Element2);
            Elements = new MenuText(Text.FormatKey("MENU_TEAM_ELEMENT", typeString), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 1 + TitledStripMenu.TITLE_OFFSET), origElements ? Color.White : Color.Yellow);

            Level = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT", player.Level), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2 + 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET));


            HP = new MenuText(Text.FormatKey("MENU_TEAM_HP", player.HP, player.MaxHP), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));
            Fullness = new MenuText(Text.FormatKey("MENU_TEAM_HUNGER", player.Fullness, player.MaxFullness), Bounds.Start + new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET));

            MainDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            SkillTitle = new MenuText(Text.FormatKey("MENU_TEAM_SKILLS"), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET));
            Skills = new MenuText[CharData.MAX_SKILL_SLOTS * 3];
            for (int ii = 0; ii < CharData.MAX_SKILL_SLOTS; ii++)
            {
                SlotSkill skill = player.BaseSkills[ii];
                string skillString = "-----";
                string skillCharges = "--";
                string totalCharges = "/--";
                if (skill.SkillNum > -1)
                {
                    SkillData data = DataManager.Instance.GetSkill(skill.SkillNum);
                    ElementData element = DataManager.Instance.GetElement(data.Data.Element);
                    skillString = String.Format("{0}\u2060{1}", element.Symbol, data.Name.ToLocal());
                    skillCharges = skill.Charges.ToString();
                    totalCharges = "/" + (data.BaseCharges + player.ChargeBoost);
                }
                Skills[ii * 3] = new MenuText(skillString, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (ii + 5) + TitledStripMenu.TITLE_OFFSET));
                Skills[ii * 3 + 1] = new MenuText(skillCharges, new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2 - 16 - GraphicsManager.TextFont.CharSpace, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (ii + 5) + TitledStripMenu.TITLE_OFFSET), DirH.Right);
                Skills[ii * 3 + 2] = new MenuText(totalCharges, new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2 - 16, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (ii + 5) + TitledStripMenu.TITLE_OFFSET), DirH.Left);
            }

            IntrinsicDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            bool origIntrinsic = (player.Intrinsics[0].Element.ID == player.BaseIntrinsics[0]);
            IntrinsicData entry = DataManager.Instance.GetIntrinsic(player.Intrinsics[0].Element.ID);
            Intrinsic = new MenuText(Text.FormatKey("MENU_TEAM_INTRINSIC", entry.Name.ToLocal()), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), origIntrinsic ? Color.White : Color.Yellow);
            IntrinsicDesc = new DialogueText(entry.Desc.ToLocal(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 3 - Bounds.X, LINE_SPACE, false);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;

            yield return Portrait;
            yield return Name;
            
            yield return Level;
            yield return Elements;
            yield return HP;
            yield return Fullness;

            yield return MainDiv;

            yield return SkillTitle;
            foreach (MenuText skill in Skills)
                yield return skill;

            yield return IntrinsicDiv;
            yield return Intrinsic;
            yield return IntrinsicDesc;
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
                MenuManager.Instance.ReplaceMenu(new MemberInfoMenu(teamSlot, assembly, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new MemberStatsMenu(teamSlot, assembly, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Up))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = (!assembly) ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot - 1 < 0)
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(amtLimit - 1, !assembly, allowAssembly));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(teamSlot - 1, assembly, allowAssembly));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu((teamSlot + DataManager.Instance.Save.ActiveTeam.Players.Count - 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly));
            }
            else if (IsInputting(input, Dir8.Down))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (allowAssembly)
                {
                    int amtLimit = assembly ? DataManager.Instance.Save.ActiveTeam.Assembly.Count : DataManager.Instance.Save.ActiveTeam.Players.Count;
                    if (teamSlot + 1 >= amtLimit)
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(0, !assembly, allowAssembly));
                    else
                        MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu(teamSlot + 1, assembly, allowAssembly));
                }
                else
                    MenuManager.Instance.ReplaceMenu(new MemberFeaturesMenu((teamSlot + 1) % DataManager.Instance.Save.ActiveTeam.Players.Count, false, allowAssembly));
            }
        }
    }
}
