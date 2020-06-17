using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueElements;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class SkillForgetMenu : TitledStripMenu
    {
        OnChooseSlot chooseSlotAction;
        Action refuseAction;
        Character player;

        SkillSummary summaryMenu;

        public SkillForgetMenu(Character player, OnChooseSlot action, Action refuseAction)
        {
            int menuWidth = 152;
            this.player = player;
            this.chooseSlotAction = action;
            this.refuseAction = refuseAction;


            List<MenuChoice> char_skills = new List<MenuChoice>();
            for (int jj = 0; jj < player.BaseSkills.Count; jj++)
            {
                SlotSkill skill = player.BaseSkills[jj];
                if (skill.SkillNum > -1)
                {
                    string skillString = Data.DataManager.Instance.GetSkill(skill.SkillNum).Name.ToLocal();
                    string skillCharges = skill.Charges + "/" + Data.DataManager.Instance.GetSkill(skill.SkillNum).BaseCharges;
                    int index = jj;
                    MenuText menuText = new MenuText(skillString, new Loc(2, 1));
                    MenuText menuCharges = new MenuText(skillCharges, new Loc(menuWidth - 8 * 4, 1), DirH.Right);
                    char_skills.Add(new MenuElementChoice(() => { choose(index); }, true, menuText, menuCharges));
                }
            }

            summaryMenu = new SkillSummary(Rect.FromPoints(new Loc(16,
                GraphicsManager.ScreenHeight - 8 - GraphicsManager.MenuBG.TileHeight * 2 - LINE_SPACE * 2 - VERT_SPACE * 4),
                new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 8)));

            Initialize(new Loc(16, 16), menuWidth, Text.FormatKey("MENU_SKILLS_TITLE", player.BaseName), char_skills.ToArray(), 0, CharData.MAX_SKILL_SLOTS);
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

            chooseSlotAction(choice);
        }

        protected override void ChoiceChanged()
        {
            summaryMenu.SetSkill(player.BaseSkills[CurrentChoice].SkillNum);
            
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
