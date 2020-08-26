using System;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class SkillSummary : SummaryMenu
    {

        DialogueText Description;
        MenuDivider MenuDiv;
        MenuText SkillElement;
        MenuText SkillCategory;
        MenuText SkillPower;
        MenuText SkillHitRate;
        MenuText Targets;

        public SkillSummary(Rect bounds) : base(bounds)
        {
            SkillElement = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(SkillElement);
            SkillCategory = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            Elements.Add(SkillCategory);

            SkillPower = new MenuText("", Bounds.Start + new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(SkillPower);
            SkillHitRate = new MenuText("", Bounds.Start + new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            Elements.Add(SkillHitRate);

            Targets = new MenuText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));
            Elements.Add(Targets);


            Description = new DialogueText("", Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, LINE_SPACE, false);
            Elements.Add(Description);

            MenuDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + LINE_SPACE),
                Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
        }

        public void SetSkill(int index)
        {
            SkillData skillEntry = DataManager.Instance.GetSkill(index);
            ElementData elementEntry = DataManager.Instance.GetElement(skillEntry.Data.Element);
            SkillElement.Text = Text.FormatKey("MENU_SKILLS_ELEMENT", String.Format("{0}\u2060{1}", elementEntry.Symbol, elementEntry.Name.ToLocal()));
            SkillCategory.Text = Text.FormatKey("MENU_SKILLS_CATEGORY", skillEntry.Data.Category.ToLocal());
            BasePowerState powerState = skillEntry.Data.SkillStates.GetWithDefault<BasePowerState>();
            SkillPower.Text = Text.FormatKey("MENU_SKILLS_POWER", (powerState != null ? powerState.Power.ToString() : "---"));
            SkillHitRate.Text = Text.FormatKey("MENU_SKILLS_HIT_RATE", (skillEntry.Data.HitRate > -1 ? skillEntry.Data.HitRate + "%" : "---"));
            Targets.Text = Text.FormatKey("MENU_SKILLS_RANGE", skillEntry.HitboxAction.GetDescription());
            Description.Text = skillEntry.Desc.ToLocal();
        }

    }
}
