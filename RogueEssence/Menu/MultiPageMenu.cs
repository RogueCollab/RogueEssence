using System;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class MultiPageMenu : TitledStripMenu
    {
        public MenuText PageText;
        public IChoosable[][] TotalChoices;
        public int CurrentPage;
        public int SpacesPerPage;
        public bool ShowPagesOnSingle;

        public int CurrentChoiceTotal { get => CurrentPage * SpacesPerPage + CurrentChoice; }

        protected void Initialize(Loc start, int width, string title, IChoosable[][] totalChoices, int defaultChoice, int defaultPage, int spacesPerPage)
        {
            Initialize(start, width, title, totalChoices, defaultChoice, defaultPage, spacesPerPage, true, -1);
        }
        protected void Initialize(Loc start, int width, string title, IChoosable[][] totalChoices, int defaultChoice, int defaultPage, int spacesPerPage, bool showPagesOnSingle, int multiSelect)
        {
            Initialize(start, width, title, totalChoices, defaultChoice, defaultPage, spacesPerPage, showPagesOnSingle, new IntRange(-1, multiSelect + 1));
        }

        protected void Initialize(Loc start, int width, string title, IChoosable[][] totalChoices, int defaultChoice, int defaultPage, int spacesPerPage, bool showPagesOnSingle, IntRange multiSelect)
        {
            TotalChoices = totalChoices;
            SpacesPerPage = spacesPerPage;
            ShowPagesOnSingle = showPagesOnSingle;
            
            Bounds = new Rect(start, new Loc(width, spacesPerPage * VERT_SPACE + GraphicsManager.MenuBG.TileHeight * 2 + ContentOffset));
            MultiSelect = multiSelect;

            IncludeTitle(title);

            PageText = new MenuText("", new Loc(width - GraphicsManager.MenuBG.TileWidth, GraphicsManager.MenuBG.TileHeight), DirH.Right);
            NonChoices.Add(PageText);

            SetPage(defaultPage);
            CurrentChoice = defaultChoice;
        }

        protected static IChoosable[][] SortIntoPages(IChoosable[] choices, int maxSlots)
        {
            int pages = MathUtils.DivUp(choices.Length, maxSlots);
            int count = 0;
            List<IChoosable[]> box = new List<IChoosable[]>();
            for (int ii = 0; ii < pages; ii++)
            {
                box.Add(new IChoosable[Math.Min(choices.Length - maxSlots * ii, maxSlots)]);
                for (int jj = 0; jj < box[ii].Length; jj++)
                {
                    box[ii][jj] = choices[count];
                    count++;
                }
            }

            return box.ToArray();
        }

        protected virtual void SetPage(int page)
        {
            CurrentPage = page;
            if (TotalChoices.Length == 1 && !ShowPagesOnSingle)
                PageText.SetText("");
            else
                PageText.SetText("(" + (CurrentPage + 1) + "/" + TotalChoices.Length+ ")");
            IChoosable[] choices = new IChoosable[TotalChoices[CurrentPage].Length];
            for (int ii = 0; ii < choices.Length; ii++)
                choices[ii] = TotalChoices[CurrentPage][ii];
            SetChoices(choices);
            CurrentChoice = Math.Min(CurrentChoice, choices.Length - 1);
        }

        protected override void UpdateKeys(InputManager input)
        {
            bool moved = false;
            if (TotalChoices.Length > 1)
            {
                if (IsInputting(input, Dir8.Left))
                {
                    SetPage((CurrentPage + TotalChoices.Length - 1) % TotalChoices.Length);
                    moved = true;
                }
                else if (IsInputting(input, Dir8.Right))
                {
                    SetPage((CurrentPage + 1) % TotalChoices.Length);
                    moved = true;
                }
            }
            if (moved)
            {
                GameManager.Instance.SE("Menu/Skip");
                cursor.ResetTimeOffset();
            }
            else if (input.JustPressed(FrameInput.InputType.Confirm))
            {
                if (MultiSelect.Max > 0)
                {
                    List<int> slots = new List<int>();
                    for (int ii = 0; ii < TotalChoices.Length; ii++)
                    {
                        for (int jj = 0; jj < TotalChoices[ii].Length; jj++)
                        {
                            if (TotalChoices[ii][jj].Selected)
                                slots.Add(ii * SpacesPerPage + jj);
                        }
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
            else
                base.UpdateKeys(input);
        }

        public IChoosable GetTotalChoiceAtIndex(int totalIndex)
        {
            int page = totalIndex / SpacesPerPage;
            int index = totalIndex % SpacesPerPage;
            return TotalChoices[page][index];
        }

        public List<IChoosable> ExportTotalChoices()
        {
            List<IChoosable> allChoices = new();
            foreach (IChoosable[] page in TotalChoices)
                foreach (IChoosable choice in page)
                    allChoices.Add(choice);
            return allChoices;
        }
        public void ImportTotalChoices(List<IChoosable> choices)
        {
            ImportTotalChoices(choices.ToArray());
        }
        public void ImportTotalChoices(IChoosable[] choices)
        {
            TotalChoices = SortIntoPages(choices, SpacesPerPage);
        }
    }
}
