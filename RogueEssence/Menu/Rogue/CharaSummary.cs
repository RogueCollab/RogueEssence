using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class CharaSummary : SummaryMenu
    {
        MenuText Details;
        MenuDivider MenuDiv;

        public CharaSummary(Rect bounds) : base(bounds)
        {
            Details = new MenuText(Text.FormatKey("MENU_CHARA_CUSTOMIZE", DiagManager.Instance.GetControlString(FrameInput.InputType.SortItems)), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Details);
            MenuDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
        }

        public void SetDetails(string formString, int skinSetting, Gender genderSetting, string intrinsicString)
        {
            while(Elements.Count > 2)
                Elements.RemoveAt(2);

            List<MenuText> rules = new List<MenuText>();

            if (formString != "")
                rules.Add(new MenuText(formString, new Loc()));
            if (skinSetting > 0)
                rules.Add(new MenuText(Text.FormatKey("MENU_CHARA_DETAIL_SKIN", DataManager.Instance.GetSkin(skinSetting).GetColoredName()), new Loc()));
            if (genderSetting != Gender.Unknown)
                rules.Add(new MenuText(genderSetting.ToLocal(), new Loc()));
            if (intrinsicString != "")
                rules.Add(new MenuText(Text.FormatKey("MENU_TEAM_INTRINSIC", intrinsicString), new Loc()));

            if (rules.Count == 0)
                rules.Add(new MenuText(Text.FormatKey("MENU_NONE"), new Loc()));

            for (int ii = 0; ii < rules.Count; ii++)
            {
                rules[ii].Loc = Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * ii);
                Elements.Add(rules[ii]);
            }

            Bounds.Size = new Loc(Bounds.Size.X, GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * rules.Count);
        }

    }
}
