using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class VertChoiceMenu : ChoiceMenu
    {
        public delegate void OnChooseSlot(int slot);
        public delegate void OnMultiChoice(List<int> slot);

        private int currentChoice;
        public int CurrentChoice
        {
            get { return currentChoice; }
            protected set
            {
                currentChoice = value;
                cursor.Loc = new Loc(GraphicsManager.MenuBG.TileWidth * 2 - 7, GraphicsManager.MenuBG.TileHeight + CurrentChoice * VERT_SPACE + ContentOffset);
                ChoiceChanged();
            }
        }

        public virtual int ContentOffset { get { return 0; } }

        private int hoveredChoice;
        private bool clicking;
        public IntRange MultiSelect { get; protected set; }
        private int selectedTotal;

        public virtual bool CanMenu { get { return true; } }
        public virtual bool CanCancel { get { return true; } }

        protected void Initialize(Loc start, int width, IChoosable[] choices, int defaultChoice)
        {
            Initialize(start, width, choices, defaultChoice, choices.Length, -1);
        }

        protected void Initialize(Loc start, int width, IChoosable[] choices, int defaultChoice, int totalSpaces, int multiSelect)
        {
            Initialize(start, width, choices, defaultChoice, totalSpaces, new IntRange(-1, multiSelect+1));
        }

        protected void Initialize(Loc start, int width, IChoosable[] choices, int defaultChoice, int totalSpaces, IntRange multiSelect)
        {
            Bounds = new Rect(start, new Loc(width, choices.Length * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + ContentOffset));

            MultiSelect = multiSelect;

            SetChoices(choices);
            CurrentChoice = defaultChoice;
        }

        protected void SetChoices(IChoosable[] choices)
        {
            Choices.Clear();
            for (int ii = 0; ii < choices.Length; ii++)
            {
                Choices.Add(choices[ii]);
                choices[ii].Bounds = new Rect(new Loc(GraphicsManager.MenuBG.TileWidth + 16 - 5, GraphicsManager.MenuBG.TileHeight + ContentOffset + VERT_SPACE * ii - 1),
                    new Loc(Bounds.Width - GraphicsManager.MenuBG.TileWidth * 2 - 16 + 5 - 4, VERT_SPACE - 2));
            }
        }
        protected int CalculateChoiceLength(IEnumerable<MenuTextChoice> choices, int minWidth)
        {
            int maxWidth = minWidth;
            foreach(MenuTextChoice choice in choices)
                maxWidth = Math.Max(choice.Text.GetTextLength() + 16 + GraphicsManager.MenuBG.TileWidth * 2, maxWidth);
            maxWidth = MathUtils.DivUp(maxWidth, 4) * 4;
            return maxWidth;
        }

        protected virtual void ChoiceChanged() { }

        public override void Update(InputManager input)
        {
            UpdateMouse(input);

            if (!clicking)
                UpdateKeys(input);
        }

        protected virtual void UpdateMouse(InputManager input)
        {
            //when moused down on a selection, change currentchoice to that choice
            //find the choice it's hovered over
            int newHover = FindHoveredMenuChoice(input);
            if (hoveredChoice != newHover)
            {
                if (hoveredChoice > -1 && hoveredChoice < Choices.Count)
                    Choices[hoveredChoice].OnHoverChanged(false);
                if (newHover > -1)
                    Choices[newHover].OnHoverChanged(true);
                hoveredChoice = newHover;
            }
            if (input.JustPressed(FrameInput.InputType.LeftMouse))
            {
                if (newHover > -1)
                {
                    CurrentChoice = newHover;
                    clicking = true;
                }

                foreach (IChoosable choice in Choices)
                    choice.OnMouseState(true);
            }
            else if (input.JustReleased(FrameInput.InputType.LeftMouse))
            {
                clicking = false;
                foreach (IChoosable choice in Choices)
                    choice.OnMouseState(false);
            }
        }


        protected virtual void UpdateKeys(InputManager input)
        {
            if (input.JustPressed(FrameInput.InputType.Confirm))
            {
                if (MultiSelect.Max > 0)
                {
                    List<int> slots = new List<int>();
                    for (int ii = 0; ii < Choices.Count; ii++)
                    {
                        if (Choices[ii].Selected)
                            slots.Add(ii);
                    }
                    if (slots.Count >= MultiSelect.Min)
                    {
                        if (slots.Count > 0)
                        {
                            GameManager.Instance.SE("Menu/Confirm");
                            ChoseMultiIndex(slots);
                        }
                        else
                            Choices[CurrentChoice].OnConfirm();
                    }
                    else
                        GameManager.Instance.SE("Menu/Cancel");
                }
                else
                    Choices[CurrentChoice].OnConfirm();
            }
            else if (input.JustPressed(FrameInput.InputType.Menu))
            {
                if (CanMenu)
                {
                    GameManager.Instance.SE("Menu/Cancel");
                    MenuPressed();
                }
            }
            else if (input.JustPressed(FrameInput.InputType.Cancel))
            {
                if (CanCancel)
                {
                    GameManager.Instance.SE("Menu/Cancel");
                    Canceled();
                }
            }
            else if (MultiSelect.Max > 0 && input.JustPressed(FrameInput.InputType.SelectItems))
            {
                int spaceLeft = MultiSelect.Max - 1 - selectedTotal;
                if (spaceLeft > 0 || Choices[CurrentChoice].Selected)
                {
                    Choices[CurrentChoice].OnSelect(!Choices[CurrentChoice].Selected);
                    if (Choices[CurrentChoice].Selected)
                        selectedTotal++;
                    else
                        selectedTotal--;
                }
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }
            else
            {
                bool moved = false;
                if (Choices.Count > 1)
                {
                    if (IsInputting(input, Dir8.Down, Dir8.DownLeft, Dir8.DownRight))
                    {
                        moved = true;
                        CurrentChoice = (CurrentChoice + 1) % Choices.Count;
                    }
                    else if (IsInputting(input, Dir8.Up, Dir8.UpLeft, Dir8.UpRight))
                    {
                        moved = true;
                        CurrentChoice = (CurrentChoice + Choices.Count - 1) % Choices.Count;
                    }
                    if (moved)
                    {
                        GameManager.Instance.SE("Menu/Select");
                        cursor.ResetTimeOffset();
                    }
                }
            }
        }

        private int FindHoveredMenuChoice(InputManager input)
        {
            for (int ii = Choices.Count - 1; ii >= 0; ii--)
            {
                if (Collision.InBounds(Choices[ii].Bounds, input.MouseLoc / GraphicsManager.WindowZoom - Bounds.Start))
                    return ii;
            }
            return -1;
        }

        protected abstract void MenuPressed();
        protected abstract void Canceled();
        protected abstract void ChoseMultiIndex(List<int> slots);
    }

    public abstract class SingleStripMenu : VertChoiceMenu
    {
        protected override void MenuPressed()
        {
            MenuManager.Instance.ClearToCheckpoint();
        }

        protected override void Canceled()
        {
            MenuManager.Instance.RemoveMenu();
        }
        protected override void ChoseMultiIndex(List<int> slots) { }
    }
}
