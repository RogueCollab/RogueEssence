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
            SkillElement = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(SkillElement);
            SkillCategory = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            Elements.Add(SkillCategory);

            SkillPower = new MenuText("", new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight));
            Elements.Add(SkillPower);
            SkillHitRate = new MenuText("", new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            Elements.Add(SkillHitRate);

            Targets = new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));
            Elements.Add(Targets);


            Description = new DialogueText("", new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 4)), LINE_HEIGHT);
            Elements.Add(Description);

            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);
        }

        public void SetSkill(int index)
        {
            SkillData skillEntry = DataManager.Instance.GetSkill(index);
            ElementData elementEntry = DataManager.Instance.GetElement(skillEntry.Data.Element);
            SkillElement.SetText(Text.FormatKey("MENU_SKILLS_ELEMENT", elementEntry.GetIconName()));
            SkillCategory.SetText(Text.FormatKey("MENU_SKILLS_CATEGORY", skillEntry.Data.Category.ToLocal()));
            BasePowerState powerState = skillEntry.Data.SkillStates.GetWithDefault<BasePowerState>();
            SkillPower.SetText(Text.FormatKey("MENU_SKILLS_POWER", (powerState != null ? powerState.Power.ToString() : "---")));
            SkillHitRate.SetText(Text.FormatKey("MENU_SKILLS_HIT_RATE", (skillEntry.Data.HitRate > -1 ? skillEntry.Data.HitRate + "%" : "---")));
            Targets.SetText(Text.FormatKey("MENU_SKILLS_RANGE", skillEntry.HitboxAction.GetDescription()));
            Description.SetFormattedText(skillEntry.Desc.ToLocal());
        }

    }
}
