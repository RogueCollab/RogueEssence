using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System;

namespace RogueEssence.Menu
{
    public class TrailResultsMenu : SideScrollMenu
    {
        public const int MAX_LINES = 13;

        public MenuText Title;
        public MenuDivider Div;
        public MenuText[] TrailPoints;

        public GameProgress Ending;
        public int Page;

        public TrailResultsMenu(GameProgress ending, int page)
        {
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 140, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 140, 224));
            Ending = ending;
            Page = page;

            List<string> trailData = ending.Trail;
            Title = new MenuText(Text.FormatKey("MENU_TRAIL_TITLE", Page + 1, MathUtils.DivUp(trailData.Count, MAX_LINES)), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            int displayTotal = Math.Min(MAX_LINES, trailData.Count - Page * MAX_LINES);
            TrailPoints = new MenuText[displayTotal];
            for (int ii = 0; ii < displayTotal; ii++)
            {
                TrailPoints[ii] = new MenuText(trailData[ii + Page * MAX_LINES], new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET));
            }

            base.Initialize();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;

            foreach (MenuText item in TrailPoints)
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
                    MenuManager.Instance.ReplaceMenu(new TrailResultsMenu(Ending, Page - 1));
                else
                {
                    int eligibleAssemblyCount = AssemblyResultsMenu.GetEligibleCount(Ending);
                    if (eligibleAssemblyCount > 0)
                        MenuManager.Instance.ReplaceMenu(new AssemblyResultsMenu(Ending, (eligibleAssemblyCount - 1) / 4));
                    else
                        MenuManager.Instance.ReplaceMenu(new PartyResultsMenu(Ending));
                }
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (Page < (Ending.Trail.Count - 1) / MAX_LINES)
                    MenuManager.Instance.ReplaceMenu(new TrailResultsMenu(Ending, Page + 1));
                else
                    MenuManager.Instance.ReplaceMenu(new VersionResultsMenu(Ending, 0));
            }

        }
    }
}
