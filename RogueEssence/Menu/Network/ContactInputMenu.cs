using System;
using Microsoft.Xna.Framework.Input;
using SDL2;

namespace RogueEssence.Menu
{
    public class ContactInputMenu : TextInputMenu
    {
        public const int MAX_LENGTH = 240;
        public const int UUID_LENGTH = 36;
        public override int MaxLength { get { return MAX_LENGTH; } }
        public override int MaxCharLength { get { return UUID_LENGTH; } }

        OnChooseText chooseTextAction;

        public ContactInputMenu(OnChooseText action) : this(MenuLabel.CONTACT_ADD_MENU, action) { }
        public ContactInputMenu(string label, OnChooseText action)
        {
            Label = label;
            chooseTextAction = action;

            Initialize(RogueEssence.Text.FormatKey("INPUT_CONTACT_TITLE"), RogueEssence.Text.FormatKey("INPUT_CAN_PASTE"), 296);
        }

        protected override void Confirmed()
        {
            //enter will advance
            if (Text.Text.Length == MaxCharLength)
            {
                GameManager.Instance.SE("Menu/Confirm");
                MenuManager.Instance.RemoveMenu();
                chooseTextAction(Text.Text);
            }
            else
                GameManager.Instance.SE("Menu/Cancel");
        }



        public override void Update(InputManager input)
        {

            bool[] charInput = new bool[TOTAL_CHARS];
            
            for (int ii = 0; ii < 6; ii++)
            {
                //check all letters
                if (input.BaseKeyPressed((Keys)(ii + 65)))
                    charInput['A' - 32 + ii] = true;
            }
            for (int ii = 0; ii < 10; ii++)
            {
                if (input.BaseKeyPressed((Keys)(48 + ii)))
                    charInput['0' - 32 + ii] = true;
                if (input.BaseKeyPressed((Keys)(96 + ii)))
                    charInput['0' - 32 + ii] = true;
            }
            if (input.BaseKeyPressed(Keys.OemMinus))
                charInput['-' - 32] = true;
            if (input.BaseKeyPressed(Keys.Subtract))
                charInput['-' - 32] = true;

            bool pressEsc = input.BaseKeyPressed(Keys.Escape) || input.BaseButtonPressed(Buttons.Back);
            bool pressEnter = input.BaseKeyPressed(Keys.Enter) || input.BaseButtonPressed(Buttons.Start);
            bool pressBack = input.BaseKeyPressed(Keys.Back);


            if (PressedPaste(input))
            {
                Guid resultGuid;
                if (Guid.TryParse(SDL.SDL_GetClipboardText(), out resultGuid))
                {
                    Text.SetText(resultGuid.ToString().ToUpper());
                    UpdatePickerPos();
                    GameManager.Instance.SE("Menu/Sort");
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else if (pressEnter)
                Confirmed();
            else if (pressEsc)
                Canceled();
            else if (pressBack)
            {
                //backspace will erase (if there's something there)
                if (Text.Text != "")
                    Text.SetText(Text.Text.Substring(0, Text.Text.Length - 1));
                GameManager.Instance.SE("Menu/Cancel");
                UpdatePickerPos();
            }
            else
                ProcessTextInput(GetRecentChars(charInput));
        }
    }
}
