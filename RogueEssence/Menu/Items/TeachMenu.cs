using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class TeachMenu : TitledStripMenu
    {
        SummaryMenu summaryMenu;
        MenuText SummaryTitle;
        MenuText[] Skills;
        MenuText[] SkillCharges;

        private int invSlot;

        public TeachMenu(int invSlot)
        {
            this.invSlot = invSlot;

            List<MenuTextChoice> team = new List<MenuTextChoice>();
            foreach (Character character in DungeonScene.Instance.ActiveTeam.Players)
            {
                bool canLearn = CanLearnSkill(character, DungeonScene.Instance.FocusedCharacter, invSlot) && !character.Dead;
                int teamIndex = team.Count;
                team.Add(new MenuTextChoice(character.BaseName, () => { choose(teamIndex); }, canLearn, canLearn ? Color.White : Color.Red));
            }

            Loc summaryStart = new Loc(16, 16 + team.Count * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + ContentOffset);
            summaryMenu = new SummaryMenu(new Rect(summaryStart, new Loc(144, CharData.MAX_SKILL_SLOTS * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + ContentOffset)));

            SummaryTitle = new MenuText("", summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight));
            summaryMenu.Elements.Add(SummaryTitle);
            summaryMenu.Elements.Add(new MenuDivider(summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), 144 - GraphicsManager.MenuBG.TileWidth * 2));
            Skills = new MenuText[CharData.MAX_SKILL_SLOTS];
            SkillCharges = new MenuText[CharData.MAX_SKILL_SLOTS];
            for (int ii = 0; ii < Skills.Length; ii++)
            {
                Skills[ii] = new MenuText("", summaryMenu.Bounds.Start + new Loc(GraphicsManager.MenuBG.TileWidth + 8, GraphicsManager.MenuBG.TileHeight + ContentOffset + VERT_SPACE * ii));
                summaryMenu.Elements.Add(Skills[ii]);
                SkillCharges[ii] = new MenuText("", new Loc(summaryMenu.Bounds.End.X - GraphicsManager.MenuBG.TileWidth, summaryMenu.Bounds.Y + GraphicsManager.MenuBG.TileHeight + ContentOffset + VERT_SPACE * ii), DirH.Right);
                summaryMenu.Elements.Add(SkillCharges[ii]);
            }

            Initialize(new Loc(16, 16), 144, Text.FormatKey("MENU_TEACH_TITLE"), team.ToArray(), 0);
        }

        public static bool CanLearnSkill(Character character, Character user, int invSlot)
        {
            Data.BaseMonsterForm entry = Data.DataManager.Instance.GetMonster(character.BaseForm.Species).Forms[character.BaseForm.Form];
            int itemNum = -1;
            if (invSlot > BattleContext.EQUIP_ITEM_SLOT)
                itemNum = DungeonScene.Instance.ActiveTeam.Inventory[invSlot].ID;
            else if (invSlot == BattleContext.EQUIP_ITEM_SLOT)
                itemNum = user.EquippedItem.ID;
            else if (invSlot == BattleContext.FLOOR_ITEM_SLOT)
            {
                //item on the ground
                int mapSlot = ZoneManager.Instance.CurrentMap.GetItem(user.CharLoc);
                MapItem mapItem = ZoneManager.Instance.CurrentMap.Items[mapSlot];
                itemNum = mapItem.Value;
            }
                
            Data.ItemData itemData = Data.DataManager.Instance.GetItem(itemNum);
            ItemIndexState effect = itemData.ItemStates.Get<ItemIndexState>();

            //check for already knowing the skill
            for(int ii = 0; ii < character.BaseSkills.Count; ii++)
            {
                if (character.BaseSkills[ii].SkillNum == effect.Index)
                    return false;
            }

            if (!Data.DataManager.Instance.DataIndices[Data.DataManager.DataType.Skill].Entries[effect.Index].Released)
                return false;

            return entry.TeachSkills.Contains(new Data.LearnableSkill(effect.Index));
        }

        protected override void ChoiceChanged()
        {
            Character character = DungeonScene.Instance.ActiveTeam.Players[CurrentChoice];
            SummaryTitle.Text = Text.FormatKey("MENU_SKILLS_TITLE", character.BaseName);
            for (int ii = 0; ii < Skills.Length; ii++)
            {
                if (character.BaseSkills[ii].SkillNum > -1)
                {
                    Skills[ii].Text = Data.DataManager.Instance.GetSkill(character.BaseSkills[ii].SkillNum).Name.ToLocal();
                    SkillCharges[ii].Text = character.BaseSkills[ii].Charges + "/" + Data.DataManager.Instance.GetSkill(character.BaseSkills[ii].SkillNum).BaseCharges;
                }
                else
                {
                    Skills[ii].Text = "";
                    SkillCharges[ii].Text = "";
                }
            }
            base.ChoiceChanged();
        }

        private void choose(int choice)
        {
            MenuManager.Instance.ClearMenus();
            //give the item at the inv slot to the given team slot
            MenuManager.Instance.EndAction = DungeonScene.Instance.ProcessPlayerInput(new GameAction(GameAction.ActionType.UseItem, Dir8.None, invSlot, choice));
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
