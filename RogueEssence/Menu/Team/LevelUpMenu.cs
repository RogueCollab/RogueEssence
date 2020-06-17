using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class LevelUpMenu : InteractableMenu
    {
        public MenuText Level;
        public MenuText HP;
        public MenuText Speed;
        public MenuText Atk;
        public MenuText Def;
        public MenuText MAtk;
        public MenuText MDef;

        public LevelUpMenu(int teamIndex, int oldLevel, int oldHP, int oldSpeed, int oldAtk, int oldDef, int oldMAtk, int oldMDef)
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 80, 24), new Loc(GraphicsManager.ScreenWidth / 2 + 80, 160));

            //TODO: align this text properly
            Character player = DungeonScene.Instance.ActiveTeam.Players[teamIndex];
            Level = new MenuText(Text.FormatKey("MENU_TEAM_LEVEL")+"    " + oldLevel + "    +" + (player.Level - oldLevel) + "    " + player.Level,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.Right);
            HP = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.HP.ToLocal("tiny")) + "    " + oldHP + "    +" + (player.MaxHP - oldHP) + "    " + player.MaxHP,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2), DirH.Right);
            Atk = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.Attack.ToLocal("tiny")) + "    " + oldAtk + "    +" + (player.BaseAtk - oldAtk) + "    " + player.BaseAtk,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3), DirH.Right);
            Def = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.Defense.ToLocal("tiny")) + "    " + oldDef + "    +" + (player.BaseDef - oldDef) + "    " + player.BaseDef,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4), DirH.Right);
            MAtk = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.MAtk.ToLocal("tiny")) + "    " + oldMAtk + "    +" + (player.BaseMAtk - oldMAtk) + "    " + player.BaseMAtk,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5), DirH.Right);
            MDef = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.MDef.ToLocal("tiny")) + "    " + oldMDef + "    +" + (player.BaseMDef - oldMDef) + "    " + player.BaseMDef,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6), DirH.Right);
            Speed = new MenuText(Text.FormatKey("MENU_LABEL", Data.Stat.Speed.ToLocal("tiny")) + "    " + oldSpeed + "    +" + (player.Speed - oldSpeed) + "    " + player.Speed,
                new Loc(GraphicsManager.ScreenWidth / 2 + 56, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7), DirH.Right);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Level;
            yield return HP;
            yield return Speed;
            yield return Atk;
            yield return Def;
            yield return MAtk;
            yield return MDef;
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
