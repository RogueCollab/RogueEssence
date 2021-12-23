using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Menu
{
    public class KeyControlsMenu : MultiPageMenu
    {
        private const int SLOTS_PER_PAGE = 12;
        
        Keys[] dirKeys;
        Keys[] actionKeys;
        bool enter;
        bool numpad;

        public KeyControlsMenu()
        {

            dirKeys = new Keys[DiagManager.Instance.CurSettings.DirKeys.Length];
            actionKeys = new Keys[DiagManager.Instance.CurSettings.ActionKeys.Length];
            enter = DiagManager.Instance.CurSettings.Enter;
            numpad = DiagManager.Instance.CurSettings.NumPad;

            DiagManager.Instance.CurSettings.DirKeys.CopyTo(dirKeys, 0);
            DiagManager.Instance.CurSettings.ActionKeys.CopyTo(actionKeys, 0);

            List<MenuChoice> flatChoices = new List<MenuChoice>();

            for (int ii = 0; ii < actionKeys.Length; ii++)
            {
                int index = ii;
                if (!Settings.UsedByKeyboard((FrameInput.InputType)index))
                    continue;
                MenuText buttonName = new MenuText(((FrameInput.InputType)index).ToLocal(), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText("[" + actionKeys[index].ToLocal() + "]", new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { chooseAction(index, buttonType); }, true, buttonName, buttonType));
            }
            for (int ii = 0; ii < dirKeys.Length; ii++)
            {
                int index = remapDirIndex(ii);
                MenuText buttonName = new MenuText(((Dir4)index).ToLocal(), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText("[" + dirKeys[index].ToLocal() + "]", new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { chooseDir(index, buttonType); }, true, buttonName, buttonType));
            }
            {
                MenuText buttonName = new MenuText(Keys.Enter.ToLocal(), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText(enter ? Text.FormatKey("DLG_CHOICE_ON") : Text.FormatKey("DLG_CHOICE_OFF"), new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { toggleEnter(); }, true, buttonName, buttonType));
            }
            {
                MenuText buttonName = new MenuText(Keys.NumPad0.ToLocal().Replace("0", ""), new Loc(2, 1), Color.White);
                MenuText buttonType = new MenuText(enter ? Text.FormatKey("DLG_CHOICE_ON") : Text.FormatKey("DLG_CHOICE_OFF"), new Loc(200, 1), DirH.Right);
                flatChoices.Add(new MenuElementChoice(() => { toggleNumpad(); }, true, buttonName, buttonType));
            }
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_RESET"), resetDefaults));
            flatChoices.Add(new MenuTextChoice(Text.FormatKey("MENU_CONTROLS_CONFIRM"), confirm));
            IChoosable[][] choices = SortIntoPages(flatChoices.ToArray(), SLOTS_PER_PAGE);

            Initialize(new Loc(16, 16), 232, Text.FormatKey("MENU_KEYBOARD_TITLE"), choices, 0, 0, SLOTS_PER_PAGE);
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
            
            for (int ii = 0; ii < actionKeys.Length; ii++)
            {
                int index = ii;

                if (!Settings.UsedByKeyboard((FrameInput.InputType)index))
                    continue;

                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText("[" + actionKeys[index].ToLocal() + "]");
                if (actionConflicts(index))
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Red;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Red;
                    conflicted = true;
                }
                else if (actionKeys[index] != DiagManager.Instance.CurSettings.ActionKeys[index])
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

            for (int ii = 0; ii < dirKeys.Length; ii++)
            {
                int index = remapDirIndex(ii);

                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText("[" + dirKeys[index].ToLocal() + "]");
                if (dirConflicts(index))
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Red;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Red;
                    conflicted = true;
                }
                else if (dirKeys[index] != DiagManager.Instance.CurSettings.DirKeys[index])
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

            //enter
            {
                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText(enter ? Text.FormatKey("DLG_CHOICE_ON") : Text.FormatKey("DLG_CHOICE_OFF"));
                if (enterConflicts())
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Red;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Red;
                    conflicted = true;
                }
                else if (enter != DiagManager.Instance.CurSettings.Enter)
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

            //numpad
            {
                IChoosable choice = TotalChoices[totalIndex / SLOTS_PER_PAGE][totalIndex % SLOTS_PER_PAGE];
                ((MenuText)((MenuElementChoice)choice).Elements[1]).SetText(numpad ? Text.FormatKey("DLG_CHOICE_ON") : Text.FormatKey("DLG_CHOICE_OFF"));
                if (numpadConflicts())
                {
                    ((MenuText)((MenuElementChoice)choice).Elements[0]).Color = Color.Red;
                    ((MenuText)((MenuElementChoice)choice).Elements[1]).Color = Color.Red;
                    conflicted = true;
                }
                else if (numpad != DiagManager.Instance.CurSettings.NumPad)
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

        private bool checkForKey(Keys[] keys, Keys checkKey, int exclude)
        {
            for (int ii = 0; ii < keys.Length; ii++)
            {
                if (ii != exclude && keys[ii] == checkKey)
                    return true;
            }
            return false;
        }

        private bool enterConflicts()
        {
            if (enter)
            {
                if (checkForKey(dirKeys, Keys.Enter, -1))
                    return true;
                if (checkForKey(actionKeys, Keys.Enter, -1))
                    return true;
            }
            return false;
        }

        private bool numpadConflicts()
        {
            if (numpad)
            {
                for (int ii = 0; ii < dirKeys.Length; ii++)
                {
                    if (dirKeys[ii] >= Keys.NumPad0 && dirKeys[ii] <= Keys.NumPad9)
                        return true;
                }
                for (int ii = 0; ii < actionKeys.Length; ii++)
                {
                    if (actionKeys[ii] >= Keys.NumPad0 && actionKeys[ii] <= Keys.NumPad9)
                        return true;
                }
            }
            return false;
        }

        private bool dirConflicts(int index)
        {
            //must not have the same key as any other dir or action
            if (checkForKey(dirKeys, dirKeys[index], index))
                return true;
            if (checkForKey(actionKeys, dirKeys[index], -1))
                return true;
            //must not be enter or numpad if those are turned on
            if (enter)
            {
                if (dirKeys[index] == Keys.Enter)
                    return true;
            }
            if (numpad)
            {
                if (dirKeys[index] >= Keys.NumPad0 && dirKeys[index] <= Keys.NumPad9)
                    return true;
            }

            return false;
        }

        private bool actionConflicts(int index)
        {
            //must not have the samekey as any direction
            if (checkForKey(dirKeys, actionKeys[index], index))
                return true;
            //also must not have the same key as any group it belongs to
            for (int ii = 0; ii < actionKeys.Length; ii++)
            {
                if (ii != index && actionKeys[ii] == actionKeys[index])
                {
                    if (Settings.MenuConflicts.Contains((FrameInput.InputType)index) && Settings.MenuConflicts.Contains((FrameInput.InputType)ii)
                        || Settings.DungeonConflicts.Contains((FrameInput.InputType)index) && Settings.DungeonConflicts.Contains((FrameInput.InputType)ii)
                        || Settings.ActionConflicts.Contains((FrameInput.InputType)index) && Settings.ActionConflicts.Contains((FrameInput.InputType)ii))
                        return true;
                }
            }
            if (enter)
            {
                if (actionKeys[index] == Keys.Enter)
                    return true;
            }
            if (numpad)
            {
                if (actionKeys[index] >= Keys.NumPad0 && actionKeys[index] <= Keys.NumPad9)
                    return true;
            }
            return false;
        }

        private void chooseDir(int index, MenuText buttonType)
        {
            buttonType.SetText("["+Text.FormatKey("MENU_CONTROLS_CHOOSE_KEY") +"]");

            MenuManager.Instance.AddMenu(new GetKeyMenu(Settings.ForbiddenKeys, (Keys key) =>
            {
                dirKeys[index] = key;
                refresh();
            }, () => { refresh(); }), true);

        }

        private void chooseAction(int index, MenuText buttonType)
        {
            buttonType.SetText("[" + Text.FormatKey("MENU_CONTROLS_CHOOSE_KEY") + "]");

            MenuManager.Instance.AddMenu(new GetKeyMenu(Settings.ForbiddenKeys, (Keys key) =>
            {
                actionKeys[index] = key;
                refresh();
            }, () => { refresh(); }), true);

        }

        private void toggleEnter()
        {
            enter = !enter;

            refresh();
        }

        private void toggleNumpad()
        {
            numpad = !numpad;

            refresh();
        }

        private void resetDefaults()
        {
            Settings.DefaultControls(dirKeys, actionKeys, null);

            refresh();
        }

        private void confirm()
        {
            dirKeys.CopyTo(DiagManager.Instance.CurSettings.DirKeys, 0);
            actionKeys.CopyTo(DiagManager.Instance.CurSettings.ActionKeys, 0);
            DiagManager.Instance.CurSettings.Enter = enter;
            DiagManager.Instance.CurSettings.NumPad = numpad;

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
