using System;
using System.Globalization;
using Microsoft.Xna.Framework.Input;
using SDL2;

namespace RogueEssence.Menu
{
    public class SeedInputMenu : TextInputMenu
    {
        public const int MAX_LENGTH = 104;
        public const int SEED_LENGTH = 16;
        public override int MaxLength { get { return MAX_LENGTH; } }
        public override int MaxCharLength { get { return SEED_LENGTH; } }

        OnChooseText chooseTextAction;

        public SeedInputMenu(OnChooseText action, ulong? seed)
        {
            chooseTextAction = action;

            Initialize(RogueEssence.Text.FormatKey("INPUT_SEED_TITLE"), RogueEssence.Text.FormatKey("INPUT_CAN_PASTE"), 296);

            if (seed.HasValue)
                Text.Text = seed.Value.ToString("X");
        }

        protected override void Confirmed()
        {
            //enter will advance
            GameManager.Instance.SE("Menu/Confirm");
            MenuManager.Instance.RemoveMenu();
            chooseTextAction(Text.Text);
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

            bool holdCtrl = (input.BaseKeyDown(Keys.LeftControl) || input.BaseKeyDown(Keys.RightControl));
            bool pressEsc = input.BaseKeyPressed(Keys.Escape) || input.BaseButtonPressed(Buttons.Back);
            bool pressEnter = input.BaseKeyPressed(Keys.Enter) || input.BaseButtonPressed(Buttons.Start);
            bool pressBack = input.BaseKeyPressed(Keys.Back);


            if (input.BaseKeyPressed(Keys.V) && holdCtrl)
            {
                ulong seed;
                if (UInt64.TryParse(SDL.SDL_GetClipboardText(), NumberStyles.HexNumber, null, out seed))
                {
                    Text.Text = seed.ToString("X");
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
                    Text.Text = Text.Text.Substring(0, Text.Text.Length - 1);
                GameManager.Instance.SE("Menu/Cancel");
                UpdatePickerPos();
            }
            else
                ProcessTextInput(GetRecentChars(charInput));
        }
    }
}
