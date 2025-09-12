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

        public CharaSummary(Rect bounds) : this(MenuLabel.CHARA_SUMMARY, bounds) { }
        public CharaSummary(string label, Rect bounds) : base(bounds)
        {
            Label = label;
            Details = new MenuText(Text.FormatKey("MENU_CHARA_CUSTOMIZE", DiagManager.Instance.GetControlString(FrameInput.InputType.SortItems)), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(Details);
            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
        }

        public void SetDetails(string formString, string skinSetting, Gender genderSetting, string intrinsicString)
        {
            while(Elements.Count > 2)
                Elements.RemoveAt(2);

            List<MenuText> rules = new List<MenuText>();

            if (formString != "")
                rules.Add(new MenuText(formString, Loc.Zero));
            if (skinSetting != DataManager.Instance.DefaultSkin)
                rules.Add(new MenuText(Text.FormatKey("MENU_CHARA_DETAIL_SKIN", DataManager.Instance.GetSkin(skinSetting).GetColoredName()), Loc.Zero));
            if (genderSetting != Gender.Unknown)
                rules.Add(new MenuText(genderSetting.ToLocal(), Loc.Zero));
            if (intrinsicString != "")
                rules.Add(new MenuText(Text.FormatKey("MENU_TEAM_INTRINSIC", intrinsicString), Loc.Zero));

            if (rules.Count == 0)
                rules.Add(new MenuText(Text.FormatKey("MENU_NONE"), Loc.Zero));

            for (int ii = 0; ii < rules.Count; ii++)
            {
                rules[ii].Loc = new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * ii);
                Elements.Add(rules[ii]);
            }

            Bounds.Size = new Loc(Bounds.Size.X, GraphicsManager.MenuBG.TileHeight * 2 + TitledStripMenu.TITLE_OFFSET + VERT_SPACE * rules.Count);
        }

    }
}
