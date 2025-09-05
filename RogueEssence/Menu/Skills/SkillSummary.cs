﻿using RogueElements;
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
        MenuText SkillCharges;


        public SkillSummary(Rect bounds) : this(MenuLabel.SKILL_SUMMARY, bounds) { }
        public SkillSummary(string label, Rect bounds) : base(bounds)
        {
            Label = label;
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
            
            SkillCharges = new MenuText("", new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            Elements.Add(SkillCharges);
            
            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2 + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);
            Elements.Add(MenuDiv);

            Description = new DialogueText("", new Rect(new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3),
                new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 4, Bounds.Height - GraphicsManager.MenuBG.TileHeight * 4)), LINE_HEIGHT);
            Elements.Add(Description);

        }

        public void SetSkill(string index)
        {
            SkillData skillEntry = DataManager.Instance.GetSkill(index);
            ElementData elementEntry = DataManager.Instance.GetElement(skillEntry.Data.Element);
            SkillElement.SetText(Text.FormatKey("MENU_SKILLS_ELEMENT", elementEntry.GetIconName()));
            SkillCategory.SetText(Text.FormatKey("MENU_SKILLS_CATEGORY", skillEntry.Data.Category.ToLocal()));
            BasePowerState powerState = skillEntry.Data.SkillStates.GetWithDefault<BasePowerState>();
            SkillPower.SetText(Text.FormatKey("MENU_SKILLS_POWER", (powerState != null ? powerState.Power.ToString() : "---")));
            SkillHitRate.SetText(Text.FormatKey("MENU_SKILLS_HIT_RATE", (skillEntry.Data.HitRate > -1 ? skillEntry.Data.HitRate + "%" : "---")));
            Targets.SetText(Text.FormatKey("MENU_SKILLS_RANGE", skillEntry.HitboxAction.GetDescription()));
            SkillCharges.SetText(Text.FormatKey("MENU_SKILLS_TOTAL_CHARGES", skillEntry.BaseCharges));
            Description.SetAndFormatText(skillEntry.Desc.ToLocal());
        }

    }
}