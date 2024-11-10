using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class VersionDiffMenu : SideScrollMenu
    {
        public const int MAX_LINES = 13;

        public MenuText Title;
        public MenuDivider Div;
        public MenuText[][] Versions;

        public List<ModDiff> ModDiffs;
        public int Page;

        public VersionDiffMenu(List<ModDiff> modDiffs, int page)
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 140, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 140, 224));
            ModDiffs = modDiffs;
            Page = page;

            Title = new MenuText(Text.FormatKey("MENU_VERSION_TITLE", Page + 1, MathUtils.DivUp(ModDiffs.Count, MAX_LINES)), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            int displayTotal = Math.Min(MAX_LINES, ModDiffs.Count - Page * MAX_LINES);
            Versions = new MenuText[displayTotal][];
            for (int ii = 0; ii < displayTotal; ii++)
            {
                Versions[ii] = new MenuText[3];
                Versions[ii][0] = new MenuText(ModDiffs[ii + Page * MAX_LINES].Name, new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET));
                Versions[ii][1] = new MenuText(ModDiffs[ii + Page * MAX_LINES].OldVersionString + " \u2192", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 48, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET), DirH.Right);
                Versions[ii][2] = new MenuText(ModDiffs[ii + Page * MAX_LINES].NewVersionString, new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET), DirH.Right);
            }

            base.Initialize();
        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return Title;
            yield return Div;

            foreach (MenuText[] arr in Versions)
                foreach (MenuText item in arr)
                    yield return item;
        }

        public override void Update(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.Menu) || input.JustPressed(FrameInput.InputType.Confirm)
                || input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
            }
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Page > 0)
                    MenuManager.Instance.ReplaceMenu(new VersionDiffMenu(ModDiffs, Page - 1));
                else
                    MenuManager.Instance.ReplaceMenu(new VersionDiffMenu(ModDiffs, (ModDiffs.Count - 1) / VersionDiffMenu.MAX_LINES));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Page < (ModDiffs.Count - 1) / MAX_LINES)
                    MenuManager.Instance.ReplaceMenu(new VersionDiffMenu(ModDiffs, Page + 1));
                else
                    MenuManager.Instance.ReplaceMenu(new VersionDiffMenu(ModDiffs, 0));
            }

        }
    }
}
