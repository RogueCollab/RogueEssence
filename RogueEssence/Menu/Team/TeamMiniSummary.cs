using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class TeamMiniSummary : SummaryMenu
    {
        MenuText FullName;
        MenuText Level;
        MenuText HP;
        MenuText Fullness;
        MenuText EXP;
        MenuText Intrinsics;
        
        public TeamMiniSummary(Rect bounds)
            : base(bounds)
        {
            FullName = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(FullName);
            Level = new MenuText("", new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Elements.Add(Level);
            HP = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            Elements.Add(HP);

            Fullness = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));
            Elements.Add(Fullness);
            EXP = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3));
            Elements.Add(EXP);
            Intrinsics = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4));
            Elements.Add(Intrinsics);
        }

        public void SetMember(Character character)
        {
            FullName.Text = character.BaseName + " / " + character.FullFormName;
            Level.Text = Text.FormatKey("MENU_TEAM_LEVEL_SHORT", character.Level);
            HP.Text = Text.FormatKey("MENU_TEAM_HP", character.HP, character.MaxHP);
            Fullness.Text = Text.FormatKey("MENU_TEAM_HUNGER", character.Fullness, character.MaxFullness);

            int expToNext = 0;
            if (character.Level < DataManager.Instance.MaxLevel)
            {
                int growth = DataManager.Instance.GetMonster(character.BaseForm.Species).EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                expToNext = growthData.GetExpToNext(character.Level);
            }
            EXP.Text = Text.FormatKey("MENU_TEAM_EXP", character.EXP, expToNext);
            Intrinsics.Text = Text.FormatKey("MENU_TEAM_INTRINSIC", (character.Intrinsics[0].Element.ID > -1 ? DataManager.Instance.GetIntrinsic(character.Intrinsics[0].Element.ID).Name.ToLocal() : DataManager.Instance.GetIntrinsic(0).Name.ToLocal()));
        }
    }
}
