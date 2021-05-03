using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class TeachInfoMenu : InteractableMenu
    {
        MenuText SkillName;
        MenuText SkillCharges;

        DialogueText Description;
        MenuDivider MenuDiv;
        MenuText SkillElement;
        MenuText SkillCategory;
        MenuText SkillPower;
        MenuText SkillHitRate;
        MenuText SkillTargets;

        public TeachInfoMenu(int itemNum)
        {
            Bounds = Rect.FromPoints(new Loc(16, 24), new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 72));
            
            ItemData itemData = DataManager.Instance.GetItem(itemNum);
            ItemIndexState effect = itemData.ItemStates.GetWithDefault<ItemIndexState>();

            SkillData skillEntry = DataManager.Instance.GetSkill(effect.Index);
            ElementData elementEntry = DataManager.Instance.GetElement(skillEntry.Data.Element);

            SkillName = new MenuText(skillEntry.GetColoredName(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            SkillCharges = new MenuText(Text.FormatKey("MENU_SKILLS_TOTAL_CHARGES", skillEntry.BaseCharges), Bounds.Start + new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight));

            SkillElement = new MenuText(Text.FormatKey("MENU_SKILLS_ELEMENT", elementEntry.GetIconName()), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            SkillCategory = new MenuText(Text.FormatKey("MENU_SKILLS_CATEGORY", skillEntry.Data.Category.ToLocal()), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));

            BasePowerState powerState = skillEntry.Data.SkillStates.GetWithDefault<BasePowerState>();
            SkillPower = new MenuText(Text.FormatKey("MENU_SKILLS_POWER", (powerState != null ? powerState.Power.ToString() : "---")), Bounds.Start + new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            SkillHitRate = new MenuText(Text.FormatKey("MENU_SKILLS_HIT_RATE", (skillEntry.Data.HitRate > -1 ? skillEntry.Data.HitRate + "%" : "---")), Bounds.Start + new Loc((Bounds.End.X - Bounds.X) / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));

            SkillTargets = new MenuText(Text.FormatKey("MENU_SKILLS_RANGE", skillEntry.HitboxAction.GetDescription()), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3));


            Description = new DialogueText(skillEntry.Desc.ToLocal(), Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4),
                Bounds.End.X - GraphicsManager.MenuBG.TileWidth * 4 - Bounds.X, LINE_SPACE, false);

            MenuDiv = new MenuDivider(Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + LINE_SPACE),
                Bounds.End.X - Bounds.X - GraphicsManager.MenuBG.TileWidth * 2);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return SkillName;
            yield return SkillCharges;

            yield return Description;
            yield return MenuDiv;

            yield return SkillElement;
            yield return SkillCategory;
            yield return SkillPower;
            yield return SkillHitRate;
            yield return SkillTargets;
        }


        public override void Update(InputManager input)
        {
            Visible = true;
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.ClearMenus();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
            }
        }
    }
}
