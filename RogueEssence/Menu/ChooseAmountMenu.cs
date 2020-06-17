using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using System;

namespace RogueEssence.Menu
{
    public abstract class ChooseAmountMenu : InteractableMenu
    {
        protected const int CURSOR_FLASH_TIME = 24;

        public List<IMenuElement> NonChoices;
        public MenuDigits Digits;
        public const int DIGIT_SEP = 14;
        public int Min;
        public int Max;
        public MenuDivider[] DigitLines;

        protected int CurrentChoice;

        public ulong PrevTick;

        public ChooseAmountMenu()
        {
            NonChoices = new List<IMenuElement>();
        }

        protected void Initialize(Rect bounds, MenuDigits digits, int min, int max, int defaultDigit)
        {
            Bounds = bounds;
            Digits = digits;
            Min = min;
            Max = max;
            CurrentChoice = defaultDigit;
            DigitLines = new MenuDivider[digits.MinDigits];
            for (int ii = 0; ii < DigitLines.Length; ii++)
                DigitLines[ii] = new MenuDivider(Digits.GetDigitLoc(ii) + new Loc(-1, 12), MenuDigits.DIGIT_SPACE - 1);
        }

        public int GetChoiceCount() { return Digits.GetDigitLength(); }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Digits;
            foreach (IMenuElement nonChoice in NonChoices)
                yield return nonChoice;
            foreach (MenuDivider div in DigitLines)
                yield return div;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw cursor
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || Inactive)
            {
                Loc pos = Digits.GetDigitLoc(CurrentChoice) + new Loc(-2, 2);
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(pos.X, pos.Y - DIGIT_SEP), 1, 0, Color.White, SpriteEffects.FlipVertically);
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(pos.X, pos.Y + DIGIT_SEP), 1, 0);
            }
        }

        public override void Update(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.Menu))
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuPressed();
            }
            else if (input.JustPressed(FrameInput.InputType.Confirm))
            {
                GameManager.Instance.SE("Menu/Confirm");
                Confirmed();
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                GameManager.Instance.SE("Menu/Cancel");
                Canceled();
            }
            else
            {
                bool moved = false;
                int choiceCount = GetChoiceCount();
                if (choiceCount > 0)
                {
                    if (IsInputting(input, Dir8.Left))
                    {
                        moved = true;
                        GameManager.Instance.SE("Menu/Select");
                        CurrentChoice = (CurrentChoice + choiceCount - 1) % choiceCount;
                    }
                    else if (IsInputting(input, Dir8.Right))
                    {
                        moved = true;
                        GameManager.Instance.SE("Menu/Select");
                        CurrentChoice = (CurrentChoice + 1) % choiceCount;
                    }
                    else if (IsInputting(input, Dir8.Up))
                    {
                        moved = true;
                        if (Digits.Amount == Max)
                            GameManager.Instance.SE("Menu/Cancel");
                        else
                        {
                            GameManager.Instance.SE("Menu/Skip");
                            int amt = 1;
                            for (int ii = 0; ii < choiceCount - CurrentChoice - 1; ii++)
                                amt *= 10;
                            Digits.Amount = Math.Min(Max, Digits.Amount + amt);
                            ValueChanged();
                        }
                    }
                    else if (IsInputting(input, Dir8.Down))
                    {
                        moved = true;
                        if (Digits.Amount == Min)
                            GameManager.Instance.SE("Menu/Cancel");
                        else
                        {
                            GameManager.Instance.SE("Menu/Skip");
                            int amt = 1;
                            for (int ii = 0; ii < choiceCount - CurrentChoice - 1; ii++)
                                amt *= 10;
                            Digits.Amount = Math.Max(Min, Digits.Amount - amt);
                            ValueChanged();
                        }
                    }

                    if (moved)
                        PrevTick = GraphicsManager.TotalFrameTick % (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME);
                }
            }
        }


        protected virtual void MenuPressed()
        {
            MenuManager.Instance.ClearMenus();
        }

        protected virtual void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
        }

        protected virtual void ValueChanged() { }

        protected abstract void Confirmed();
    }
}
