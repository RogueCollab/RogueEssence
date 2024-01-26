using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class LevelUpMenu : InteractableMenu
    {
        public MenuText Title;
        public MenuDivider Div;

        public MenuText[] Level;
        public MenuText[] HP;
        public MenuText[] Speed;
        public MenuText[] Atk;
        public MenuText[] Def;
        public MenuText[] MAtk;
        public MenuText[] MDef;
        public MenuDivider[] Divs;

        public LevelUpMenu(int teamIndex, int oldLevel, int oldHP, int oldSpeed, int oldAtk, int oldDef, int oldMAtk, int oldMDef) :
            this(DungeonScene.Instance.ActiveTeam.Players[teamIndex], oldLevel, oldHP, oldSpeed, oldAtk, oldDef, oldMAtk, oldMDef)
        {
        }

        public LevelUpMenu(Character player, int oldLevel, int oldHP, int oldSpeed, int oldAtk, int oldDef, int oldMAtk,
            int oldMDef)
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 88, 32), new Loc(GraphicsManager.ScreenWidth / 2 + 88, 180));

            Title = new MenuText(Text.FormatKey("MENU_LEVEL_UP", player.GetDisplayName(true)), new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Level = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Text.FormatKey("MENU_TEAM_LEVEL")), oldLevel, player.Level - oldLevel, player.Level);
            HP = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Data.Stat.HP.ToLocal("tiny")), oldHP, player.MaxHP - oldHP, player.MaxHP);
            Atk = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Data.Stat.Attack.ToLocal("tiny")), oldAtk, player.BaseAtk - oldAtk, player.BaseAtk);
            Def = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Data.Stat.Defense.ToLocal("tiny")), oldDef, player.BaseDef - oldDef, player.BaseDef);
            MAtk = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Data.Stat.MAtk.ToLocal("tiny")), oldMAtk, player.BaseMAtk - oldMAtk, player.BaseMAtk);
            MDef = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Data.Stat.MDef.ToLocal("tiny")), oldMDef, player.BaseMDef - oldMDef, player.BaseMDef);
            Speed = genMenuTier(GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET, Text.FormatKey("MENU_LABEL", Data.Stat.Speed.ToLocal("tiny")), oldSpeed, player.BaseSpeed - oldSpeed, player.BaseSpeed);

            Divs = new MenuDivider[6];
            for (int ii = 0; ii < 6; ii++)
                Divs[ii] = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT + VERT_SPACE * (ii+1) + TitledStripMenu.TITLE_OFFSET), Bounds.Width - 8 * 4);
        }

        private MenuText[] genMenuTier(int height, string label, int oldVal, int diff, int newVal)
        {
            List<MenuText> texts = new List<MenuText>();

            texts.Add(new MenuText(label, new Loc(GraphicsManager.MenuBG.TileWidth * 2, height), DirH.Left));
            texts.Add(new MenuText(oldVal.ToString(), new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 64, height), DirH.Right));
            string precede = "";
            if (diff > 0)
                precede = "+";
            texts.Add(new MenuText(precede+diff.ToString(), new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 32, height), DirH.Right));
            texts.Add(new MenuText(newVal.ToString(), new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2, height), DirH.Right));

            return texts.ToArray();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;

            foreach (MenuText txt in Level)
                yield return txt;
            foreach (MenuText txt in HP)
                yield return txt;
            foreach (MenuText txt in Atk)
                yield return txt;
            foreach (MenuText txt in Def)
                yield return txt;
            foreach (MenuText txt in MAtk)
                yield return txt;
            foreach (MenuText txt in MDef)
                yield return txt;
            foreach (MenuText txt in Speed)
                yield return txt;
            foreach (MenuDivider div in Divs)
                yield return div;
        }

        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu) || input.JustPressed(FrameInput.InputType.Confirm)
                || input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
            }

        }
    }
}
