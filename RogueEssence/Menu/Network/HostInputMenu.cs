using System;
using Microsoft.Xna.Framework.Input;
using SDL2;

namespace RogueEssence.Menu
{
    public class HostInputMenu : TextInputMenu
    {
        public const int MAX_LENGTH = 160;
        public override int MaxLength { get { return MAX_LENGTH; } }

        OnChooseText chooseTextAction;

        public HostInputMenu(OnChooseText action, string title, string subtitle)
        {
            chooseTextAction = action;
            Initialize(title, subtitle, 296);
        }

        protected override void Confirmed()
        {
            //enter will advance
            if (isValidHostPort(Text.Text))
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
            bool shift = (input.BaseKeyDown(Keys.LeftShift) || input.BaseKeyDown(Keys.RightShift));
            //plus shift
            int offset = (shift ? 0 : 32);
            for (int ii = 0; ii < 26; ii++)
            {
                //check all letters
                if (input.BaseKeyPressed((Keys)(ii + 65)))
                    charInput['A' - 32 + offset + ii] = true;
            }

            for (int ii = 0; ii < 10; ii++)
            {
                if (input.BaseKeyPressed((Keys)(48 + ii)))
                {
                    if (!shift)
                        charInput['0' - 32 + ii] = true;
                }
                //numpad
                if (input.BaseKeyPressed((Keys)(96 + ii)))
                    charInput['0' - 32 + ii] = true;
            }
            if (input.BaseKeyPressed(Keys.OemMinus))
                charInput['-' - 32] = true;
            if (input.BaseKeyPressed(Keys.Subtract))
                charInput['-' - 32] = true;
            if (input.BaseKeyPressed(Keys.OemSemicolon) && shift)
                charInput[(shift ? ':' : ';') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemPeriod) && !shift)
                charInput[(shift ? '>' : '.') - 32] = true;
            if (input.BaseKeyPressed(Keys.Decimal))
                charInput['.' - 32] = true;


            bool holdCtrl = (input.BaseKeyDown(Keys.LeftControl) || input.BaseKeyDown(Keys.RightControl));
            bool pressEsc = input.BaseKeyPressed(Keys.Escape) || input.BaseButtonPressed(Buttons.Back);
            bool pressEnter = input.BaseKeyPressed(Keys.Enter) || input.BaseButtonPressed(Buttons.Start);
            bool pressBack = input.BaseKeyPressed(Keys.Back);


            if (input.BaseKeyPressed(Keys.V) && holdCtrl)
            {
                string paste = SDL.SDL_GetClipboardText();
                if (isValidHostPort(paste))
                {
                    Text.Text = paste;
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

        private bool isValidHostPort(string str)
        {
            string[] hostnameport = str.Split(':');
            if (hostnameport.Length > 2)
                return false;
            ushort port;
            if (hostnameport.Length == 2 && !ushort.TryParse(hostnameport[1], out port))
                return false;
            return Uri.CheckHostName(hostnameport[0]) != UriHostNameType.Unknown;
        }
    }
}
