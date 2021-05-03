using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Menu
{
    public class GamepadControlsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;

        Buttons[] actionButtons;

        public GamepadControlsMenu()
        {
            actionButtons = new Buttons[DiagManager.Instance.CurSettings.ActionButtons.Length];

            DiagManager.Instance.CurSettings.ActionButtons.CopyTo(actionButtons, 0);

            List<MenuChoice> flatChoices = new List<MenuChoice>();

            for (int ii = 0; ii < actionButtons.Length; ii++)
            {
                int index = ii;
                if (!Settings.UsedByGamepad((FrameInput.InputType)index))
                    continue;
                MenuText buttonName = new MenuText(((FrameInput.InputType)index).ToLocal(), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText("(" + actionButtons[index].ToLocal() + ")", new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { chooseAction(index, buttonType); }, true, buttonName, buttonType));
            }

            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_RESET"), resetDefaults));
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_CONFIRM"), confirm));
            List<MenuChoice[]> choices = SortIntoPages(flatChoices, SLOTS_PER_PAGE);

            Initialize(new Loc(16, 16), 232, Text.FormatKey("MENU_KEYBOARD_TITLE"), choices.ToArray(), 0, 0, SLOTS_PER_PAGE);
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
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText("(" + actionButtons[index].ToLocal() + ")");
                if (actionConflicts(index))
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Red;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Red;
                    conflicted = true;
                }
                else if (actionButtons[index] != DiagManager.Instance.CurSettings.ActionButtons[index])
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

            totalIndex++;
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

        private void confirm()
        {
            actionButtons.CopyTo(DiagManager.Instance.CurSettings.ActionButtons, 0);

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
