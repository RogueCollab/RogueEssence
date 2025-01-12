using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using Microsoft.Xna.Framework;

namespace RogueEssence.Menu
{
    public class FinalResultsMenu : SideScrollMenu
    {
        public MenuText Title;
        public MenuText Team;
        public DialogueText Description;
        public MenuDivider Div;
        public MenuText MoneyTally;
        public MenuText InvValueTally;
        public MenuText BankValueTally;
        public MenuText TotalTally;
        public MenuText TotalTurns;
        public MenuText TotalDungeonTime;
        public MenuText Seed;
        public GameProgress Ending;

        public FinalResultsMenu(GameProgress ending) : this(MenuLabel.RESULTS_MENU_RESULT, ending) { }
        public FinalResultsMenu(string label, GameProgress ending)
        {
            Label = label;
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 140, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 140, 224));
            Ending = ending;

            Title = new MenuText(Text.FormatKey("MENU_RESULTS_TITLE"),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight), DirH.None);

            Div = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            Team = new MenuText(Ending.ActiveTeam.GetDisplayName(), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE + TitledStripMenu.TITLE_OFFSET), DirH.None);
            string message = "";
            switch (Ending.Outcome)
            {
                case GameProgress.ResultType.Cleared:
                    message = Text.FormatKey("MENU_RESULTS_COMPLETE", Ending.Location);
                    break;
                case GameProgress.ResultType.Rescue:
                    message = Text.FormatKey("MENU_RESULTS_RESCUE", Ending.Location);
                    break;
                case GameProgress.ResultType.Failed:
                case GameProgress.ResultType.Downed:
                    message = Text.FormatKey("MENU_RESULTS_DEFEAT", Ending.Location);
                    break;
                case GameProgress.ResultType.TimedOut:
                    message = Text.FormatKey("MENU_RESULTS_TIMEOUT", Ending.Location);
                    break;
                case GameProgress.ResultType.Escaped:
                    message = Text.FormatKey("MENU_RESULTS_ESCAPE", Ending.Location);
                    break;
                case GameProgress.ResultType.GaveUp:
                    message = Text.FormatKey("MENU_RESULTS_QUIT", Ending.Location);
                    break;
                default:
                    {
                        if (String.IsNullOrWhiteSpace(Ending.Location))
                            message = Text.FormatKey("MENU_RESULTS_VANISHED");
                        else
                            message = Text.FormatKey("MENU_RESULTS_VANISHED_AT", Ending.Location);
                        break;
                    }
            }

            Description = new DialogueText(message,
                new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + TitledStripMenu.TITLE_OFFSET + LINE_HEIGHT * 2)),
                LINE_HEIGHT, true, false, -1);

            MoneyTally = new MenuText(Text.FormatKey("MENU_BAG_MONEY", Text.FormatKey("MONEY_AMOUNT", Ending.ActiveTeam.Money)),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            InvValueTally = new MenuText(Text.FormatKey("MENU_RESULTS_INV_VALUE", Text.FormatKey("MONEY_AMOUNT", Ending.ActiveTeam.GetInvValue())),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 5 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            TotalTurns = new MenuText(Text.FormatKey("MENU_RESULTS_TOTAL_TURNS", Ending.TotalTurns),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 9 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            TotalDungeonTime = new MenuText(Text.FormatKey("MENU_TIMER", Ending.GetDungeonTimeDisplay()),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 10 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            Seed = new MenuText(Text.FormatKey("MENU_RESULTS_SEED", Ending.Rand.FirstSeed.ToString("X")),
                new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 11 + TitledStripMenu.TITLE_OFFSET), DirH.None);

            RogueProgress rogue = Ending as RogueProgress;

            if (rogue != null)
            {
                BankValueTally = new MenuText(Text.FormatKey("MENU_RESULTS_BONUS_VALUE", Ending.ActiveTeam.Bank + Ending.ActiveTeam.GetStorageValue()),
                    new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), DirH.None);
                TotalTally = new MenuText(Text.FormatKey("MENU_RESULTS_TOTAL_SCORE", Ending.GetTotalScore()),
                    new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), DirH.None);
                Seed.Color = rogue.Seeded ? TextBlue : Color.White;
            }
            else
            {
                BankValueTally = new MenuText("",
                    new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 6 + TitledStripMenu.TITLE_OFFSET), DirH.None);
                TotalTally = new MenuText("",
                    new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 7 + TitledStripMenu.TITLE_OFFSET), DirH.None);
            }

            base.Initialize();
        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return Title;
            yield return Team;
            yield return Description;
            yield return Div;
            yield return MoneyTally;
            yield return InvValueTally;
            yield return BankValueTally;
            yield return TotalTally;
            yield return TotalTurns;
            yield return TotalDungeonTime;
            yield return Seed;
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

                MenuManager.Instance.ReplaceMenu(new VersionResultsMenu(Ending, (Ending.GetModVersion().Count - 1) / VersionResultsMenu.MAX_LINES));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                MenuManager.Instance.ReplaceMenu(new InvResultsMenu(Ending, 0));
            }

        }
    }
}
