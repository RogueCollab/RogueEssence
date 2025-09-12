using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace RogueEssence.Menu
{
    public class GamepadControlsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;

        Buttons[] actionButtons;
        bool inactiveInput;

        public GamepadControlsMenu() : this(MenuLabel.GAMEPAD_MENU) { }
        public GamepadControlsMenu(string label)
        {
            Label = label;
            actionButtons = new Buttons[DiagManager.Instance.CurActionButtons.Length];
            DiagManager.Instance.CurActionButtons.CopyTo(actionButtons, 0);
            inactiveInput = DiagManager.Instance.CurSettings.InactiveInput;

            List<MenuChoice> flatChoices = new List<MenuChoice>();

            for (int ii = 0; ii < actionButtons.Length; ii++)
            {
                int index = ii;
                if (!Settings.UsedByGamepad((FrameInput.InputType)index))
                    continue;
                MenuText buttonName = new MenuText(((FrameInput.InputType)index).ToLocal(), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText(DiagManager.Instance.GetButtonString(actionButtons[index]), new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { chooseAction(index, buttonType); }, true, buttonName, buttonType));
            }

            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_RESET"), resetDefaults));
            {
                MenuText buttonName = new MenuText(Text.FormatKey("MENU_CONTROLS_INACTIVE"), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText(inactiveInput ? Text.FormatKey("DLG_CHOICE_ON") : Text.FormatKey("DLG_CHOICE_OFF"), new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { toggleInactiveInput(); }, true, buttonName, buttonType));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_CONFIRM"), confirm));
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            Initialize(new Loc(16, 16), 232, String.Format("{0}: {1}", Text.FormatKey("MENU_GAMEPAD_TITLE"), DiagManager.Instance.CurGamePadName), choices, 0, 0, SLOTS_PER_PAGE);
        }


        private int remapDirIndex(int ii)
        {
            int index = ii;
            if (ii == 0)
                index = 2;
            else if (ii == 1)
                index = 0;
            else if (ii == 2)
                index = 1;
            return index;
        }

        private void refresh()
        {
            bool conflicted = false;
            int totalIndex = 0;

            for (int ii = 0; ii < actionButtons.Length; ii++)
            {
                int index = ii;

                if (!Settings.UsedByGamepad((FrameInput.InputType)index))
                    continue;

                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText(DiagManager.Instance.GetButtonString(actionButtons[index]));
                if (actionConflicts(index))
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Red;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Red;
                    conflicted = true;
                }
                else if (actionButtons[index] != DiagManager.Instance.CurActionButtons[index])
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Yellow;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Yellow;
                }
                else
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.White;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.White;
                }
                totalIndex++;
            }

            //move past Reset Defaults
            totalIndex++;
            //inactive indow
            {
                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText(inactiveInput ? Text.FormatKey("DLG_CHOICE_ON") : Text.FormatKey("DLG_CHOICE_OFF"));
                if (inactiveInput != DiagManager.Instance.CurSettings.InactiveInput)
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Yellow;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Yellow;
                }
                else
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.White;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.White;
                }
                totalIndex++;
            }
            if (conflicted)
            {
                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuTextChoice)choice).Enabled = false;
                ((MenuText)((MenuTextChoice)choice).Text).Color = Color.Red;
            }
            else
            {
                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuTextChoice)choice).Enabled = true;
                ((MenuText)((MenuTextChoice)choice).Text).Color = Color.White;
            }
        }


        private bool actionConflicts(int index)
        {
            //also must not have the same key as any group it belongs to
            for (int ii = 0; ii < actionButtons.Length; ii++)
            {
                if (ii != index && actionButtons[ii] == actionButtons[index])
                {
                    if (Settings.MenuConflicts.Contains((FrameInput.InputType)index) && Settings.MenuConflicts.Contains((FrameInput.InputType)ii)
                        || Settings.DungeonConflicts.Contains((FrameInput.InputType)index) && Settings.DungeonConflicts.Contains((FrameInput.InputType)ii)
                        || Settings.ActionConflicts.Contains((FrameInput.InputType)index) && Settings.ActionConflicts.Contains((FrameInput.InputType)ii))
                        return true;
                }
            }
            return false;
        }

        private void chooseAction(int index, MenuText buttonType)
        {
            buttonType.SetText("(" + Text.FormatKey("MENU_CONTROLS_CHOOSE_BUTTON") + ")");

            MenuManager.Instance.AddMenu(new GetButtonMenu(Settings.ForbiddenButtons, (Buttons button) =>
            {
                actionButtons[index] = button;
                refresh();
            }, () => { refresh(); }), true);

        }

        private void resetDefaults()
        {
            Settings.DefaultControls(null, null, actionButtons);

            refresh();
        }

        private void toggleInactiveInput()
        {
            inactiveInput = !inactiveInput;

            refresh();
        }

        private void confirm()
        {
            actionButtons.CopyTo(DiagManager.Instance.CurActionButtons, 0);
            DiagManager.Instance.CurSettings.InactiveInput = inactiveInput;

            DiagManager.Instance.SaveSettings(DiagManager.Instance.CurSettings);
            MenuManager.Instance.NextAction = ReturnCommand();
        }

        public IEnumerator<YieldInstruction> ReturnCommand()
        {
            yield return new WaitForFrames(1);
            MenuManager.Instance.RemoveMenu();
            yield break;
        }

    }
}
