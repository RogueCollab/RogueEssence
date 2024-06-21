using System.Collections.Generic;
using System.Linq;
using RogueElements;
using Microsoft.Xna.Framework;
using RogueEssence.Script;
using NLua;
using System.Collections;
using System;

namespace RogueEssence.Menu
{
    public class ScriptableMenu : InteractableMenu
    {
        [Obsolete("Redirects to MenuBase.Elements. Kept only for script compatibility")]
        public List<IMenuElement> MenuElements
        {
            get => Elements;
            set => Elements = value;
        }

        protected Action<InputManager> UpdateFunction;

        public ScriptableMenu(string label, int x, int y, int w, int h, Action<InputManager> updateFunction) : this(x, y, w, h, updateFunction)
        { Label = label; }
        public ScriptableMenu(int x, int y, int w, int h, Action<InputManager> updateFunction)
        {
            UpdateFunction = updateFunction;
            Bounds = new Rect(x, y, w, h);
        }

        public override void Update(InputManager input)
        {
            if (UpdateFunction != null)
            UpdateFunction(input);
        }
    }


    public class ScriptableSingleStripMenu : SingleStripMenu
    {
        public Action<InputManager> UpdateFunction;

        public Action CancelFunction;

        public Action ChoiceChangedFunction;

        public Action MultiSelectChangedFunction;

        public Action<LuaTable> MultiSelectFunction;

        public ScriptableSingleStripMenu(string label, int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, Action cancelFun) : this(label, x, y, minWidth, choicesPairs, defaultChoice, cancelFun, -1, null) { }
        public ScriptableSingleStripMenu(string label, int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, Action cancelFun, int multiSelect, Action<LuaTable> onMultiSelect) : this(label, x, y, minWidth, choicesPairs, defaultChoice, cancelFun, new IntRange(-1, multiSelect + 1), onMultiSelect) { }
        public ScriptableSingleStripMenu(string label, int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, Action cancelFun, IntRange multiSelect, Action<LuaTable> onMultiSelect) : this(x, y, minWidth, choicesPairs, defaultChoice, cancelFun, multiSelect, onMultiSelect) { Label = label; }
        public ScriptableSingleStripMenu(int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, Action cancelFun) : this(x, y, minWidth, choicesPairs, defaultChoice, cancelFun, -1, null) { }
        public ScriptableSingleStripMenu(int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, Action cancelFun, int multiSelect, Action<LuaTable> onMultiSelect) : this(x, y, minWidth, choicesPairs, defaultChoice, cancelFun, new IntRange(-1, multiSelect+1), onMultiSelect) { }
        public ScriptableSingleStripMenu(int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, Action cancelFun, IntRange multiSelect, Action<LuaTable> onMultiSelect)
        {
            CancelFunction = cancelFun;
            MultiSelectFunction = onMultiSelect;
            int? mappedDefault = null;
            //Intepret the choices from lua
            List<IChoosable> choices = new List<IChoosable>();
            IDictionaryEnumerator dict = choicesPairs.GetEnumerator();
            while (dict.MoveNext())
            {
                string choicetext = "";
                bool enabled = true;
                LuaFunction fn = null;
                if (dict.Value is string)
                {
                    choicetext = dict.Value as string;

                    choices.Add(new MenuTextChoice(choicetext, () =>
                    {
                        if (fn != null)
                            fn.Call();
                    },
                        enabled, enabled ? Color.White : Color.Red));
                }
                else if (dict.Value is LuaTable)
                {
                    LuaTable tbl = dict.Value as LuaTable;
                    choicetext = (string)tbl[1];
                    enabled = (bool)tbl[2];
                    fn = (LuaFunction)tbl[3];

                    choices.Add(new MenuTextChoice(choicetext, () =>
                    {
                        if (fn != null)
                            fn.Call();
                    },
                        enabled, enabled ? Color.White : Color.Red));
                }
                else if (dict.Value is MenuChoice)
                {
                    choices.Add((MenuChoice)dict.Value);
                }
                long choiceval = (long)dict.Key;

                if (defaultChoice.Equals(choiceval))
                    mappedDefault = choices.Count;
            }

            if (mappedDefault == null)
                mappedDefault = 0;

            int choice_width = CalculateChoiceLength(choices, minWidth);
            Initialize(new Loc(x, y), choice_width, choices.ToArray(), mappedDefault.Value, choices.Count, multiSelect);
        }

        protected override void ChoiceChanged()
        {
            if (ChoiceChangedFunction != null)
                ChoiceChangedFunction();
        }

        protected override void MultiSelectChanged()
        {
            if (MultiSelectChangedFunction != null)
                MultiSelectChangedFunction();
        }

        public override void Update(InputManager input)
        {
            base.Update(input);
            if (UpdateFunction != null)
                UpdateFunction(input);
        }

        protected override void MenuPressed()
        {
            if (CancelFunction != null)
                CancelFunction();
        }

        protected override void Canceled()
        {
            if (CancelFunction != null)
                CancelFunction();
        }

        protected override void ChoseMultiIndex(List<int> slots)
        {
            if (MultiSelectFunction != null)
            {
                LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, index) table.insert(tbl, index) end").First() as LuaFunction;
                foreach (int slot in slots)
                    addfn.Call(tbl, slot);
                MultiSelectFunction(tbl);
            }
        }
    }


    public class ScriptableMultiPageMenu : MultiPageMenu
    {
        public Action<InputManager> UpdateFunction;

        public Action CancelFunction;

        public Action MenuFunction;

        public Action ChoiceChangedFunction;

        public Action MultiSelectChangedFunction;

        public Action<LuaTable> MultiSelectFunction;

        public override bool CanMenu => MenuFunction is not null;
        public override bool CanCancel => CancelFunction is not null;

        public ScriptableMultiPageMenu(string label, Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu) : this(label, start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu, true) { }
        public ScriptableMultiPageMenu(string label, Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu, bool showPagesOnSingle) : this(label, start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu, showPagesOnSingle, -1, null) { }
        public ScriptableMultiPageMenu(string label, Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu, bool showPagesOnSingle, int multiSelect, Action<LuaTable> onMultiSelect) : this(label, start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu, showPagesOnSingle, new IntRange(-1, multiSelect + 1), onMultiSelect) { }
        public ScriptableMultiPageMenu(string label, Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu, bool showPagesOnSingle, IntRange multiSelect, Action<LuaTable> onMultiSelect) : this(start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu,showPagesOnSingle, multiSelect, onMultiSelect) { Label = label; }
        public ScriptableMultiPageMenu(Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu) : this(start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu, true) { }
        public ScriptableMultiPageMenu(Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu, bool showPagesOnSingle) : this(start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu, showPagesOnSingle, -1, null) { }
        public ScriptableMultiPageMenu(Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu, bool showPagesOnSingle, int multiSelect, Action<LuaTable> onMultiSelect) : this(start, width, title, totalChoices, defaultTotalChoice, spacesPerPage, onCancel, onMenu, showPagesOnSingle, new IntRange(-1, multiSelect + 1), onMultiSelect) { }
        public ScriptableMultiPageMenu(Loc start, int width, string title, IChoosable[] totalChoices, int defaultTotalChoice, int spacesPerPage, Action onCancel, Action onMenu, bool showPagesOnSingle, IntRange multiSelect, Action<LuaTable> onMultiSelect)
        {
            this.CancelFunction = onCancel;
            this.MenuFunction = onMenu;
            this.MultiSelectFunction = onMultiSelect;
            IChoosable[][] pagedChoices = SortIntoPages(totalChoices, spacesPerPage);
            int defaultPage = defaultTotalChoice / spacesPerPage;
            int defaultChoice = defaultTotalChoice % spacesPerPage;
            Initialize(start, width, title, pagedChoices, defaultChoice, defaultPage, spacesPerPage, showPagesOnSingle, multiSelect);
        }

        protected override void ChoiceChanged()
        {
            if (ChoiceChangedFunction != null)
                ChoiceChangedFunction();
        }

        protected override void MultiSelectChanged()
        {
            if (MultiSelectChangedFunction != null)
                MultiSelectChangedFunction();
        }

        public override void Update(InputManager input)
        {
            base.Update(input);
            if (UpdateFunction != null)
                UpdateFunction(input);
        }

        public void SetCurrentPage(int page)
        {
            page = Math.Min(Math.Max(0, page), TotalChoices.Length - 1);
            SetPage(page);
        }

        protected override void MenuPressed()
        {
            if (MenuFunction != null)
                MenuFunction();
        }

        protected override void Canceled()
        {
            if (CancelFunction != null)
                CancelFunction();
        }

        protected override void ChoseMultiIndex(List<int> slots)
        {
            if (MultiSelectFunction != null)
            { 
                LuaTable tbl = LuaEngine.Instance.RunString("return {}").First() as LuaTable;
                LuaFunction addfn = LuaEngine.Instance.RunString("return function(tbl, index) table.insert(tbl, index) end").First() as LuaFunction;
                foreach (int slot in slots)
                    addfn.Call(tbl, slot);
                MultiSelectFunction(tbl);
            }
        }
    }
}
