using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;
using System;

namespace RogueEssence.Menu
{
    public class TeamMiniSummary : SummaryMenu
    {
        MenuText FullName;
        MenuText LevelLabel;
        MenuText Level;
        MenuText HPLabel;
        MenuText HP;
        MenuText FullnessLabel;
        MenuText Fullness;
        MenuText EXP;
        MenuText Intrinsics;
        
        public TeamMiniSummary(Rect bounds)
            : base(bounds)
        {
            FullName = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(FullName);
            LevelLabel = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 6, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(LevelLabel);
            Level = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 6 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_LEVEL_SHORT")), GraphicsManager.MenuBG.TileHeight), DirH.Left);
            Elements.Add(Level);
            HPLabel = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            Elements.Add(HPLabel);
            HP = new MenuText("", new Loc( GraphicsManager.MenuBG.TileWidth * 2 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HP")) + 4, GraphicsManager.MenuBG.TileHeight + VERT_SPACE), DirH.Left);
            Elements.Add(HP);

            FullnessLabel = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));
            Elements.Add(FullnessLabel);
            Fullness = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2 + GraphicsManager.TextFont.SubstringWidth(Text.FormatKey("MENU_TEAM_HUNGER")) + 4, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2), DirH.Left);
            Elements.Add(Fullness);
            EXP = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3));
            Elements.Add(EXP);
            Intrinsics = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4));
            Elements.Add(Intrinsics);
        }

        public void SetMember(Character character)
        {
            FullName.SetText(character.GetDisplayName(true) + " / " + CharData.GetFullFormName(character.BaseForm));
            LevelLabel.SetText(Text.FormatKey("MENU_TEAM_LEVEL_SHORT"));
            Level.SetText(character.Level.ToString());
            HPLabel.SetText(Text.FormatKey("MENU_TEAM_HP"));
            HP.SetText(String.Format("{0}/{1}", character.HP, character.MaxHP));
            
            FullnessLabel.SetText(Text.FormatKey("MENU_TEAM_HUNGER"));
            Fullness.SetText(String.Format("{0}/{1}", character.Fullness, character.MaxFullness));

            int expToNext = 0;
            if (character.Level < DataManager.Instance.Start.MaxLevel)
            {
                string growth = DataManager.Instance.GetMonster(character.BaseForm.Species).EXPTable;
                GrowthData growthData = DataManager.Instance.GetGrowth(growth);
                expToNext = growthData.GetExpToNext(character.Level);
            }
            EXP.SetText(Text.FormatKey("MENU_TEAM_EXP", character.EXP, expToNext));
            Intrinsics.SetText(Text.FormatKey("MENU_TEAM_INTRINSIC", (!String.IsNullOrEmpty(character.Intrinsics[0].Element.ID) ? DataManager.Instance.GetIntrinsic(character.Intrinsics[0].Element.ID).GetColoredName() : DataManager.Instance.GetIntrinsic(DataManager.Instance.DefaultIntrinsic).GetColoredName())));
        }
    }
}
