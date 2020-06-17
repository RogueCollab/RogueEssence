using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using Microsoft.Xna.Framework.Input;
using SDL2;

namespace RogueEssence.Menu
{
    public abstract class TextInputMenu : InteractableMenu
    {
        public const int TOTAL_CHARS = 95;

        protected const int CURSOR_FLASH_TIME = 24;

        public delegate void OnChooseText(string text);

        public MenuText Title;

        public MenuText Text;
        public MenuDivider NameLine;
        public MenuText Notes;

        public ulong PrevTick;
        private Loc cursorPos;

        private Dictionary<char, char> AltChar;

        public abstract int MaxLength { get; }
        public virtual int MaxCharLength { get { return 0; } }

        public TextInputMenu()
        {
            AltChar = new Dictionary<char, char>();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Title;
            yield return Text;
            yield return NameLine;
            yield return Notes;
        }

        protected void Initialize(string title, string notes, int boxLength)
        {
            Bounds = new Rect(new Loc(GraphicsManager.ScreenWidth / 2 - boxLength / 2, 50), new Loc(boxLength, TitledStripMenu.TITLE_OFFSET + LINE_SPACE * 5 + GraphicsManager.MenuBG.TileHeight * 2));

            Title = new MenuText(title, new Loc(GraphicsManager.ScreenWidth / 2, Bounds.Y + GraphicsManager.MenuBG.TileHeight), DirH.None);

            Text = new MenuText("", new Loc(GraphicsManager.ScreenWidth / 2 - MaxLength / 2, Bounds.Y + TitledStripMenu.TITLE_OFFSET * 2));
            NameLine = new MenuDivider(new Loc(GraphicsManager.ScreenWidth / 2 - MaxLength / 2, Bounds.Y + TitledStripMenu.TITLE_OFFSET * 2 + LINE_SPACE), MaxLength);

            Notes = new MenuText(notes, new Loc(GraphicsManager.ScreenWidth / 2, Bounds.Y + TitledStripMenu.TITLE_OFFSET * 2 + LINE_SPACE * 7 / 2), DirV.None, DirH.None, Color.White);

            AddAltWheel('.', '·');
            AddAltWheel('!', '¡');
            AddAltWheel('?', '¿');

            AddAltWheel('A', 'À', 'Á', 'Â', 'Ã', 'Ä', 'Å', 'Æ');
            AddAltWheel('C', 'Ç');
            AddAltWheel('E', 'È', 'É', 'Ê', 'Ë');
            AddAltWheel('I', 'Ì', 'Í', 'Î', 'Ï');
            AddAltWheel('D', 'Ð');
            AddAltWheel('N', 'Ñ');
            AddAltWheel('O', 'Ò', 'Ó', 'Ô', 'Õ', 'Ö', 'Ø', 'Œ');
            AddAltWheel('U', 'Ù', 'Ú', 'Û', 'Ü');
            //AddAltWheel('Y', 'Ý');
            AddAltWheel('T', 'Þ');
            AddAltWheel('s', 'š', 'ß');
            AddAltWheel('a', 'à', 'á', 'â', 'ã', 'ä', 'å', 'æ', 'œ');
            AddAltWheel('c', 'ç');
            AddAltWheel('e', 'è', 'é', 'ê', 'ë');
            AddAltWheel('i', 'ì', 'í', 'î', 'ï');
            AddAltWheel('d', 'ð');
            AddAltWheel('n', 'ñ');
            AddAltWheel('o', 'ò', 'ó', 'ô', 'õ', 'ö', 'ø');
            AddAltWheel('u', 'ù', 'ú', 'û', 'ü');
            AddAltWheel('y', 'ý', 'ÿ');
            AddAltWheel('t', 'þ');

            AddAltWheel('S', 'Š');
            AddAltWheel('Z', 'Ž');
            AddAltWheel('z', 'ž');

            AddAltWheel('*', '♂', '♀');

            UpdatePickerPos();
        }

        private void AddAltWheel(params char[] chars)
        {
            for (int ii = 0; ii < chars.Length; ii++)
                AltChar[chars[ii]] = chars[(ii + 1)%chars.Length];
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
                    else
                    {
                        switch (ii)
                        {
                            case 1: charInput['!' - 32] = true; break;
                            case 2: charInput['@' - 32] = true; break;
                            case 3: charInput['#' - 32] = true; break;
                            case 4: charInput['$' - 32] = true; break;
                            case 5: charInput['%' - 32] = true; break;
                            case 6: charInput['^' - 32] = true; break;
                            case 7: charInput['&' - 32] = true; break;
                            case 8: charInput['*' - 32] = true; break;
                            case 9: charInput['(' - 32] = true; break;
                            case 0: charInput[')' - 32] = true; break;
                        }
                    }
                }
                //numpad
                if (input.BaseKeyPressed((Keys)(96 + ii)))
                    charInput['0' - 32 + ii] = true;
            }

            if (input.BaseKeyPressed(Keys.Multiply))
                charInput['*' - 32] = true;
            if (input.BaseKeyPressed(Keys.Divide))
                charInput['/' - 32] = true;
            if (input.BaseKeyPressed(Keys.Decimal))
                charInput['.' - 32] = true;
            if (input.BaseKeyPressed(Keys.Add))
                charInput['+' - 32] = true;
            if (input.BaseKeyPressed(Keys.Subtract))
                charInput['-' - 32] = true;

            if (input.BaseKeyPressed(Keys.OemSemicolon))
                charInput[(shift ? ':' : ';') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemPlus))
                charInput[(shift ? '+' : '=') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemMinus))
                charInput[(shift ? '_' : '-') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemComma))
                charInput[(shift ? '<' : ',') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemPeriod))
                charInput[(shift ? '>' : '.') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemQuestion))
                charInput[(shift ? '?' : '/') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemTilde))
                charInput[(shift ? '~' : '`') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemOpenBrackets))
                charInput[(shift ? '{' : '[') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemPipe))
                charInput[(shift ? '|' : '\\') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemCloseBrackets))
                charInput[(shift ? '}' : ']') - 32] = true;
            if (input.BaseKeyPressed(Keys.OemQuotes))
                charInput[(shift ? '"' : '\'') - 32] = true;

            //check all numbers and symbols
            if (input.BaseKeyPressed(Keys.Space))
                charInput[0] = true;

            bool holdCtrl = (input.BaseKeyDown(Keys.LeftControl) || input.BaseKeyDown(Keys.RightControl));
            bool pressAlt = (input.BaseKeyPressed(Keys.LeftAlt) || input.BaseKeyPressed(Keys.RightAlt));
            bool pressEsc = input.BaseKeyPressed(Keys.Escape) || input.BaseButtonPressed(Buttons.Back);
            bool pressEnter = input.BaseKeyPressed(Keys.Enter) || input.BaseButtonPressed(Buttons.Start);
            bool pressBack = input.BaseKeyPressed(Keys.Back);

            if (input.BaseKeyPressed(Keys.V) && holdCtrl)
            {
                string paste = GetRenderableString(SDL.SDL_GetClipboardText());
                if (paste != "")
                    ProcessTextInput(paste);
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else if (pressEnter)
                Confirmed();
            else if (pressEsc)
                Canceled();
            else if (pressAlt)
            {
                //insert will change changeable characters
                if (Text.Text.Length > 0)
                {
                    char checkChar = Text.Text[Text.Text.Length - 1];
                    if (AltChar.ContainsKey(checkChar))
                    {
                        GameManager.Instance.SE("Menu/Confirm");
                        Text.Text = Text.Text.Substring(0, Text.Text.Length - 1) + AltChar[checkChar];
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
                UpdatePickerPos();
            }
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

        private static string GetRenderableString(string inputStr)
        {
            inputStr = inputStr.Trim();
            if (inputStr.Contains("\n"))
                return "";

            string resultStr = "";
            for (int ii = 0; ii < inputStr.Length; ii++)
            {
                if (GraphicsManager.TextFont.CanRenderChar(inputStr[ii]))
                    resultStr += inputStr[ii];
            }
            return resultStr;
        }

        protected void ProcessTextInput(string inputChars)
        {
            if (inputChars != "")
            {
                //characters will be appended one by one until finished or full
                int ii;
                for (ii = 0; ii < inputChars.Length; ii++)
                {
                    Text.Text += inputChars[ii];
                    if (MaxCharLength > 0 && Text.Text.Length > MaxCharLength || Text.GetTextLength() > MaxLength)
                    {
                        Text.Text = Text.Text.Substring(0, Text.Text.Length - 1);
                        break;
                    }
                }
                if (ii > 0)
                    GameManager.Instance.SE("Menu/Confirm");
                else
                    GameManager.Instance.SE("Menu/Cancel");
                UpdatePickerPos();
            }
            else
            {
                //forbid any other button pressing
            }
        }

        protected void UpdatePickerPos()
        {
            PrevTick = GraphicsManager.TotalFrameTick % (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME);
            cursorPos = new Loc(GraphicsManager.ScreenWidth / 2 - MaxLength / 2 + Text.GetTextLength() - 2, Bounds.Y + TitledStripMenu.TITLE_OFFSET * 2 + LINE_SPACE);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw ticker
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || Inactive)
                GraphicsManager.Cursor.DrawTile(spriteBatch, cursorPos.ToVector2(), 1, 0, Color.White, SpriteEffects.FlipVertically);

        }

        protected abstract void Confirmed();

        protected virtual void Canceled()
        {
            GameManager.Instance.SE("Menu/Cancel");
            MenuManager.Instance.RemoveMenu();
        }



        protected static string GetRecentChars(bool[] charInput)
        {
            string chars = "";
            for (int ii = 0; ii < TOTAL_CHARS; ii++)
            {
                if (charInput[ii])
                    chars += (char)(ii + 32);
            }
            return chars;
        }
    }
}
