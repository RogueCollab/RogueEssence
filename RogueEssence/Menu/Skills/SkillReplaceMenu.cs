using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class SkillReplaceMenu : TitledStripMenu
    {
        OnChooseSlot learnAction;
        Action refuseAction;
        Character player;
        int skillNum;

        SkillSummary summaryMenu;

        public SkillReplaceMenu(Character player, int skillNum, OnChooseSlot learnAction, Action refuseAction)
        {
            int menuWidth = 152;
            this.player = player;
            this.skillNum = skillNum;
            this.learnAction = learnAction;
            this.refuseAction = refuseAction;

            List<MenuChoice> char_skills = new List<MenuChoice>();
            for (int ii = 0; ii < player.BaseSkills.Count; ii++)
            {
                SlotSkill skill = player.BaseSkills[ii];
                if (skill.SkillNum > -1)
                {
                    SkillData data = DataManager.Instance.GetSkill(skill.SkillNum);
                    string skillString = data.GetColoredName();
                    string skillCharges = skill.Charges + "/" + (data.BaseCharges + player.ChargeBoost);
                    int index = ii;
                    MenuText menuText = new MenuText(skillString, new Loc(2, 1));
                    MenuText menuCharges = new MenuText(skillCharges, new Loc(menuWidth - 8 * 4, 1), DirH.Right);
                    MenuDivider div = new MenuDivider(new Loc(0, LINE_SPACE), menuWidth - 8 * 4);
                    char_skills.Add(new MenuElementChoice(() => { choose(index); }, true, menuText, menuCharges, div));
                }
            }
            string newSkillString = DataManager.Instance.GetSkill(skillNum).GetColoredName();
            int maxCharges = DataManager.Instance.GetSkill(skillNum).BaseCharges + player.ChargeBoost;
            string newSkillCharges = maxCharges + "/" + maxCharges;
            MenuText newMenuText = new MenuText(newSkillString, new Loc(2, 1));
            MenuText newMenuCharges = new MenuText(newSkillCharges, new Loc(menuWidth - 8 * 4, 1), DirH.Right);
            char_skills.Add(new MenuElementChoice(() => { choose(CharData.MAX_SKILL_SLOTS); }, true, newMenuText, newMenuCharges));

            summaryMenu = new SkillSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_SPACE * 2 - VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_SKILLS_TITLE", player.GetDisplayName(true)), char_skills.ToArray(), 0);
        }

        protected override void MenuPressed()
        {

        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
            refuseAction();
        }

        private void choose(int choice)
        {
            MenuManager.Instance.RemoveMenu();

            if (choice < CharData.MAX_SKILL_SLOTS)
                learnAction(choice);
            else
                refuseAction();
        }

        protected override void ChoiceChanged()
        {
            if (CurrentChoice < 4)
                summaryMenu.SetSkill(player.BaseSkills[CurrentChoice].SkillNum);
            else
                summaryMenu.SetSkill(skillNum);
            
            base.ChoiceChanged();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw other windows
            summaryMenu.Draw(spriteBatch);
        }

    }
}
