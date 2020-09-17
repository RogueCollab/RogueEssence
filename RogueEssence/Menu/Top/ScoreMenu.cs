using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class ScoreMenu : SideScrollMenu
    {
        public MenuText Title;
        public MenuDivider Div;
        public MenuText[] Scores;
        private Dictionary<int, List<RecordHeaderData>> scoreDict;
        private int chosenZone;
        private string highlightedPath;

        public ScoreMenu(Dictionary<int, List<RecordHeaderData>> scoreDict, int chosenZone, string highlightedPath)
        {
            this.scoreDict = scoreDict;
            this.chosenZone = chosenZone;
            this.highlightedPath = highlightedPath;
            List<RecordHeaderData> scores = scoreDict[chosenZone];

            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth / 2 - 128, 16), new Loc(GraphicsManager.ScreenWidth / 2 + 128, 224));

            string zoneName = DataManager.Instance.DataIndices[DataManager.DataType.Zone].Entries[chosenZone].Name.ToLocal();
            Title = new MenuText(Text.FormatKey("MENU_SCORES_TITLE") + ": " + zoneName, Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            Div = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);

            Scores = new MenuText[scores.Count * 3];
            for (int ii = 0; ii < scores.Count; ii++)
            {
                Color color = (scores[ii].Path == highlightedPath) ? Color.Yellow : Color.White;
                Scores[ii * 3] = new MenuText((ii + 1) + ". ",
                new Loc(Bounds.X + GraphicsManager.MenuBG.TileWidth * 2 + 12, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, color);
                Scores[ii * 3 + 1] = new MenuText(scores[ii].Name,
                new Loc(Bounds.X + GraphicsManager.MenuBG.TileWidth * 2 + 12, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET), color);
                Scores[ii * 3 + 2] = new MenuText(scores[ii].Score.ToString(),
                new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight + VERT_SPACE * ii + TitledStripMenu.TITLE_OFFSET), DirV.Up, DirH.Right, color);
            }
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Div;
            foreach (MenuText score in Scores)
                yield return score;
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
            else if (IsInputting(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                int newZone = chosenZone;
                do
                {
                    newZone = (newZone + DataManager.Instance.DataIndices[DataManager.DataType.Zone].Count-1) % DataManager.Instance.DataIndices[DataManager.DataType.Zone].Count;
                }
                while (!scoreDict.ContainsKey(newZone));
                MenuManager.Instance.ReplaceMenu(new ScoreMenu(scoreDict, newZone, highlightedPath));
            }
            else if (IsInputting(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                int newZone = chosenZone;
                do {
                    newZone = (newZone + 1) % DataManager.Instance.DataIndices[DataManager.DataType.Zone].Count;
                }
                while (!scoreDict.ContainsKey(newZone));
                MenuManager.Instance.ReplaceMenu(new ScoreMenu(scoreDict, newZone, highlightedPath));
            }
        }
    }
}
