using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dungeon;

namespace RogueEssence.Menu
{
    public class TeachWhomMenu : SideScrollMenu
    {
        public enum TeachState
        {
            CanLearn = 0,
            CannotLearn = 1,
            Learned = 2
        }

        public string itemId {get; private set;}
        public string skillId {get; private set;}

        int page;
        
        MenuText SkillName;
        MenuText SkillCharges;

        MenuDivider MenuDiv;
        MenuText SkillElement;
        MenuText SkillCategory;
        MenuText SkillPower;
        MenuText SkillHitRate;
        MenuText SkillTargets;

        MenuText MemberListTitle;

        List<MenuText> MemberNames;
        List<MenuText> MemberTeachState;

        public TeachWhomMenu(string itemNum, int startPage)
        {
            page = startPage;
            itemId = itemNum;
            Bounds = Rect.FromPoints(new Loc(16, 24), new Loc(GraphicsManager.ScreenWidth - 16, GraphicsManager.ScreenHeight - 72));

            ItemData itemData = DataManager.Instance.GetItem(itemNum);
            ItemIDState effect = itemData.ItemStates.GetWithDefault<ItemIDState>();
            skillId = effect.ID;

            SkillData skillEntry = DataManager.Instance.GetSkill(effect.ID);
            ElementData elementEntry = DataManager.Instance.GetElement(skillEntry.Data.Element);

            SkillName = new MenuText(skillEntry.GetColoredName(), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight));
            SkillCharges = new MenuText(Text.FormatKey("MENU_SKILLS_TOTAL_CHARGES", skillEntry.BaseCharges), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight));
            
            SkillElement = new MenuText(Text.FormatKey("MENU_SKILLS_ELEMENT", elementEntry.GetIconName()), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            SkillCategory = new MenuText(Text.FormatKey("MENU_SKILLS_CATEGORY", skillEntry.Data.Category.ToLocal()), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));

            BasePowerState powerState = skillEntry.Data.SkillStates.GetWithDefault<BasePowerState>();
            SkillPower = new MenuText(Text.FormatKey("MENU_SKILLS_POWER", (powerState != null ? powerState.Power.ToString() : "---")), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE));
            SkillHitRate = new MenuText(Text.FormatKey("MENU_SKILLS_HIT_RATE", (skillEntry.Data.HitRate > -1 ? skillEntry.Data.HitRate + "%" : "---")), new Loc(Bounds.Width / 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 2));

            SkillTargets = new MenuText(Text.FormatKey("MENU_SKILLS_RANGE", skillEntry.HitboxAction.GetDescription()), new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3));

            MenuDiv = new MenuDivider(new Loc(GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 3 + LINE_HEIGHT),
                Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2);

            MemberListTitle = new MenuText(Text.FormatKey("MENU_SKILLS_LEARN_LIST_TITLE", skillEntry.BaseCharges) + ":", new Loc(GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight + VERT_SPACE * 4));

            MemberNames = new List<MenuText>();
            MemberTeachState = new List<MenuText>();
            for (int i=0; i<4; i++)
            {
                int y = GraphicsManager.MenuBG.TileHeight + VERT_SPACE * (5 + i);
                MemberNames.Add(new MenuText("", new Loc(GraphicsManager.MenuBG.TileWidth * 2, y)));
                MemberTeachState.Add(new MenuText("", new Loc(Bounds.Width / 2, y)));
            }
            UpdateMembers();
            base.Initialize(Bounds.Top + (Bounds.Height) / 2);
        }

        private void UpdateMembers()
        {
            int startPos = page * 4;
            EventedList<Character> team = DataManager.Instance.Save.ActiveTeam.Players;
            for(int i=0; i < MemberNames.Count; i++)
            {
                int index = startPos + i;
                if(index < team.Count)
                {
                    MemberNames[i].SetText(team[index].GetDisplayName(true));

                    string stateText = "";
                    TeachState state = GetTeachState(team[index]);
                    switch (state)
                    {
                        case TeachState.CanLearn:
                            stateText = "MENU_SKILLS_CAN_LEARN";
                            break;
                        case TeachState.CannotLearn:
                            stateText = "MENU_SKILLS_CANNOT_LEARN";
                            break;
                        case TeachState.Learned:
                            stateText = "MENU_SKILLS_LEARNED";
                            break;
                    }

                    MemberTeachState[i].SetText(Text.FormatKey(stateText));
                }
                else
                {
                    MemberNames[i].SetText("");
                    MemberTeachState[i].SetText("");
                }
            }
        }

        public TeachState GetTeachState(Character character)
        {
            BaseMonsterForm entry = DataManager.Instance.GetMonster(character.BaseForm.Species).Forms[character.BaseForm.Form];

            //check for already knowing the skill
            for (int ii = 0; ii < character.BaseSkills.Count; ii++)
            {
                if (character.BaseSkills[ii].SkillNum == skillId)
                    return TeachState.Learned;
            }

            if (!DataManager.Instance.DataIndices[DataManager.DataType.Skill].Get(skillId).Released)
                return TeachState.CannotLearn;

            if (entry.CanLearnSkill(skillId))
                return TeachState.CanLearn;
            return TeachState.CannotLearn;
        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return SkillName;
            yield return SkillCharges;

            yield return SkillElement;
            yield return SkillCategory;
            yield return SkillPower;
            yield return SkillHitRate;
            yield return SkillTargets;

            yield return MenuDiv;

            yield return MemberListTitle;

            for (int i = 0; i < MemberNames.Count; i++)
            {
                yield return MemberNames[i];
                yield return MemberTeachState[i];
            }
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
            else if (DirPressed(input, Dir8.Right))
            {
                GameManager.Instance.SE("Menu/Skip");
                if (page == (int)Math.Ceiling((double)DataManager.Instance.Save.ActiveTeam.Players.Count / 4) - 1)
                    MenuManager.Instance.ReplaceMenu(new TeachInfoMenu(itemId));
                else
                    MenuManager.Instance.ReplaceMenu(new TeachWhomMenu(itemId, page + 1));
            }
            else if (DirPressed(input, Dir8.Left))
            {
                GameManager.Instance.SE("Menu/Skip");
                if(page == 0)
                    MenuManager.Instance.ReplaceMenu(new TeachInfoMenu(itemId));
                else
                    MenuManager.Instance.ReplaceMenu(new TeachWhomMenu(itemId, page - 1));
            }
        }

        private bool DirPressed(InputManager input, Dir8 dir)
        {
            return input.Direction == dir && input.PrevDirection != dir;
        }
    }
}