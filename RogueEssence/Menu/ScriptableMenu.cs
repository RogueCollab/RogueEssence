using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using NLua;
using System.Collections;

namespace RogueEssence.Menu
{
    public class ScriptableMenu : InteractableMenu
    {
        public List<IMenuElement> MenuElements;
        public List<SummaryMenu> SummaryMenus;

        protected LuaFunction UpdateFunction;

        public ScriptableMenu(int x, int y, int w, int h, LuaFunction updateFunction)
        {
            UpdateFunction = updateFunction;
            Bounds = new Rect(x, y, w, h);
            MenuElements = new List<IMenuElement>();
            SummaryMenus = new List<SummaryMenu>();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            foreach (IMenuElement choice in MenuElements)
                yield return choice;
        }

        public override void Update(InputManager input)
        {
            UpdateFunction?.Call(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            foreach (SummaryMenu menu in SummaryMenus)
                menu.Draw(spriteBatch);
        }
    }


    public class ScriptableSingleStripMenu : SingleStripMenu
    {
        public List<SummaryMenu> SummaryMenus;

        public LuaFunction CancelFunction;

        public ScriptableSingleStripMenu(int x, int y, int minWidth, LuaTable choicesPairs, object defaultChoice, LuaFunction cancelFun)
        {
            CancelFunction = cancelFun;
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
            Initialize(new Loc(x, y), choice_width, choices.ToArray(), mappedDefault.Value);

            SummaryMenus = new List<SummaryMenu>();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            foreach (SummaryMenu menu in SummaryMenus)
                menu.Draw(spriteBatch);
        }

        protected override void MenuPressed()
        {
            if (CancelFunction != null)
                CancelFunction.Call();
        }

        protected override void Canceled()
        {
            if (CancelFunction != null)
                CancelFunction.Call();
        }

    }


}
