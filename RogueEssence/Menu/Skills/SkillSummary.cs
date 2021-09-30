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


            Description = new DialogueText("", new Rect(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3),
                new Loc(Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, Bounds.End.Y - GraphicsManager.MenuBG.TileHeight * 4 - Bounds.Y)), LINE_HEIGHT);
            Elements.Add(Description);

            MenuDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + LINE_HEIGHT),
                Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
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
