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
            Loc center = Loc.Zero;

            Bounds = Rect.FromPoints(center - new Loc(56 + GraphicsManager.MenuBG.TileWidth, 0), center + new Loc(56 + GraphicsManager.MenuBG.TileWidth, LINE_HEIGHT * 2 + GraphicsManager.MenuBG.TileHeight * 2));
            skillText = new MenuText("", Bounds.Center + new Loc(-50, GraphicsManager.MenuBG.TileHeight), DirH.Left);
            Elements.Add(skillText);
            skillElement = new MenuText("", Bounds.Center + new Loc(-48, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), DirH.Left);
            Elements.Add(skillElement);
            skillCharges = new MenuText("", Bounds.Center + new Loc(48, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT), DirH.Right);
            Elements.Add(skillCharges);
        }

        public void SetArrangement(bool diamond)
        {
            Loc center = Loc.Zero;
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

            Bounds = Rect.FromPoints(center - new Loc(56 + GraphicsManager.MenuBG.TileWidth, 0), center + new Loc(56 + GraphicsManager.MenuBG.TileWidth, LINE_HEIGHT * 2 + GraphicsManager.MenuBG.TileHeight * 2));
            skillText.Loc = new Loc(Bounds.Size.X / 2 - 50, GraphicsManager.MenuBG.TileHeight);
            skillElement.Loc = new Loc(Bounds.Size.X / 2 - 48, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT);
            skillCharges.Loc = new Loc(Bounds.Size.X / 2 + 48, GraphicsManager.MenuBG.TileHeight + LINE_HEIGHT);
        }

        public void SetSkill(string skillName, string element, int charges, int max, bool skillSealed)
        {
            if (!String.IsNullOrWhiteSpace(skillName))
            {
                Color color = (skillSealed || charges == 0) ? Color.Red : Color.White;
                skillText.SetText(DiagManager.Instance.GetControlString((FrameInput.InputType)(skillSlot + (int)FrameInput.InputType.Skill1)) + ": " + skillName);
                skillText.Color = color;
                skillCharges.SetText(charges + "/" + max);
                skillCharges.Color = color;
                ElementData elementData = DataManager.Instance.GetElement(element);
                skillElement.SetText(elementData.GetIconName());
                skillElement.Color = color;
            }
            else
            {
                skillText.SetText(DiagManager.Instance.GetControlString((FrameInput.InputType)(skillSlot + (int)FrameInput.InputType.Skill1)));
                skillText.Color = Color.Gray;
                skillCharges.SetText("");
                skillCharges.Color = Color.Gray;
                skillElement.SetText("");
                skillElement.Color = Color.Gray;
            }
        }

        public void SetPreview(bool preview)
        {
            if (skillCharges.Text != "")
                skillText.Color = preview ? Color.Yellow : skillCharges.Color;
        }

    }


    public class PreviewSkillMenu : MenuBase
    {
        private MenuText menuText;

        public PreviewSkillMenu()
        {
            menuText = new MenuText("", Loc.Zero, DirH.Right);
            Elements.Add(menuText);
        }

        public void UpdateControls()
        {
            menuText.SetText(DiagManager.Instance.GetControlString(FrameInput.InputType.SkillPreview) + " [color=#FFFF00]" + Text.FormatKey("MENU_SKILL_PREVIEW") + "[color]");
            int textLength = MathUtils.DivUp(menuText.GetTextLength(), 4) * 4;
            Bounds = Rect.FromPoints(new Loc(GraphicsManager.ScreenWidth - textLength - 16 - GraphicsManager.MenuBG.TileWidth, 24), new Loc(GraphicsManager.ScreenWidth + GraphicsManager.MenuBG.TileWidth, 24 + LINE_HEIGHT + GraphicsManager.MenuBG.TileHeight * 2));
            menuText.Loc = new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2, GraphicsManager.MenuBG.TileHeight);
        }
    }
}
