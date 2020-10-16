using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Content;
using RogueEssence.Data;

namespace RogueEssence.Menu
{
    public class HotkeyMenu : MenuBase
    {
        private MenuText skillText;
        private MenuText skillElement;
        private MenuText skillCharges;
        private int skillSlot;

        public HotkeyMenu(int skillSlot)
        {
            this.skillSlot = skillSlot;
            Loc center = new Loc();

            Bounds = Rect.FromPoints(center - new Loc(56 + GraphicsManager.MenuBG.TileWidth, 0), center + new Loc(56 + GraphicsManager.MenuBG.TileWidth, LINE_SPACE * 2 + GraphicsManager.MenuBG.TileHeight * 2));
            skillText = new MenuText("", center + new Loc(-48, GraphicsManager.MenuBG.TileHeight), DirH.Left);
            skillElement = new MenuText("", center + new Loc(-48, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), DirH.Left);
            skillCharges = new MenuText("", center + new Loc(48, GraphicsManager.MenuBG.TileHeight + LINE_SPACE), DirH.Right);
        }

        public void SetArrangement(bool diamond)
        {
            Loc center = new Loc();
            if (diamond)
            {
                if (skillSlot == 0)
                    center = new Loc(GraphicsManager.ScreenWidth / 4 - 8, GraphicsManager.ScreenHeight / 2 - 16);
                else if (skillSlot == 1)
                    center = new Loc(GraphicsManager.ScreenWidth / 2, GraphicsManager.ScreenHeight / 4 - 16);
                else if (skillSlot == 2)
                    center = new Loc(GraphicsManager.ScreenWidth / 2, GraphicsManager.ScreenHeight * 3 / 4 - 16);
                else if (skillSlot == 3)
                    center = new Loc(GraphicsManager.ScreenWidth * 3 / 4 + 8, GraphicsManager.ScreenHeight / 2 - 16);
            }
            else
            {
                if (skillSlot == 0)
                    center = new Loc(GraphicsManager.ScreenWidth / 4, GraphicsManager.ScreenHeight / 4);
                else if (skillSlot == 1)
                    center = new Loc(GraphicsManager.ScreenWidth * 3 / 4, GraphicsManager.ScreenHeight / 4);
                else if (skillSlot == 2)
                    center = new Loc(GraphicsManager.ScreenWidth / 4, GraphicsManager.ScreenHeight * 3 / 4 - 32);
                else if (skillSlot == 3)
                    center = new Loc(GraphicsManager.ScreenWidth * 3 / 4, GraphicsManager.ScreenHeight * 3 / 4 - 32);
            }

            Bounds = Rect.FromPoints(center - new Loc(56 + GraphicsManager.MenuBG.TileWidth, 0), center + new Loc(56 + GraphicsManager.MenuBG.TileWidth, LINE_SPACE * 2 + GraphicsManager.MenuBG.TileHeight * 2));
            skillText.Loc =center + new Loc(-48, GraphicsManager.MenuBG.TileHeight);
            skillElement.Loc = center + new Loc(-48, GraphicsManager.MenuBG.TileHeight + LINE_SPACE);
            skillCharges.Loc = center + new Loc(48, GraphicsManager.MenuBG.TileHeight + LINE_SPACE);
        }

        public void SetSkill(string skillName, int element, int charges, int max, bool skillSealed)
        {
            if (!String.IsNullOrWhiteSpace(skillName))
            {
                Color color = (skillSealed || charges == 0) ? Color.Red : Color.White;
                skillText.Text = DiagManager.Instance.GetControlString((FrameInput.InputType)(skillSlot + (int)FrameInput.InputType.Skill1)) + ": " + skillName;
                skillText.Color = color;
                skillCharges.Text = charges + "/" + max;
                skillCharges.Color = color;
                ElementData elementData = DataManager.Instance.GetElement(element);
                skillElement.Text = String.Format("{0}\u2060{1}", elementData.Symbol, elementData.Name.ToLocal());
                skillElement.Color = color;
            }
            else
            {
                skillText.Text = DiagManager.Instance.GetControlString((FrameInput.InputType)(skillSlot + (int)FrameInput.InputType.Skill1));
                skillText.Color = Color.Gray;
                skillCharges.Text = "";
                skillCharges.Color = Color.Gray;
                skillElement.Text = "";
                skillElement.Color = Color.Gray;
            }
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return skillText;
            yield return skillCharges;
            yield return skillElement;
        }
    }
}
