using System;
using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class MenuChoice : BaseMenuElement, IChoosable
    {
        public Rect Bounds { get; set; }

        public Action ChoiceAction;

        public bool Enabled;
        public bool Selected { get; private set; }

        private bool hover;
        private bool click;

        protected MenuChoice(string label, Action choiceAction, bool enabled)
        {
            Label = label;
            Bounds = new Rect();
            ChoiceAction = choiceAction;
            Enabled = enabled;
        }
        protected MenuChoice(Action choiceAction, bool enabled) : this("", choiceAction, enabled) { }

        //chosen by clicking
        public void OnMouseState(bool clicked)
        {
            if (click && !clicked && hover)
            {
                if (Enabled)
                {
                    if (ChoiceAction != null)
                        ChoiceAction();
                }
            }
            else if (!click && clicked && hover)
            {
                if (Enabled)
                    GameManager.Instance.SE("Menu/Confirm");
                else
                    GameManager.Instance.SE("Menu/Cancel");
            }

            this.click = clicked && hover;
        }

        public void SilentSelect(bool select)
        {
            if (Enabled)
            {
                Selected = select;
            }
        }

        public void OnSelect(bool select)
        {
            if (Enabled)
            {
                GameManager.Instance.SE("Menu/Toggle");
                Selected = select;
            }
            else
                GameManager.Instance.SE("Menu/Cancel");
        }

        //selected by mouse
        public void OnHoverChanged(bool hover)
        {
            this.hover = hover;
        }

        //chosen by confirm button
        public void OnConfirm()
        {
            if (Enabled)
            {
                GameManager.Instance.SE("Menu/Confirm");
                if (ChoiceAction != null)
                    ChoiceAction();
            }
            else
                GameManager.Instance.SE("Menu/Cancel");

        }
        public abstract IEnumerable<IMenuElement> GetElements();

        public override void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            //draw the highlight
            if (Selected)
                GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(Bounds.X + offset.X, Bounds.Y + offset.Y, Bounds.Size.X, Bounds.Size.Y), null, Color.Cyan * 0.5f);
            if (hover && Enabled)
                GraphicsManager.Pixel.Draw(spriteBatch, new Rectangle(Bounds.X + offset.X, Bounds.Y + offset.Y, Bounds.Size.X, Bounds.Size.Y), null, Color.White * (click ? 0.5f : 0.2f));
            //draw all elements with offset added
            foreach (IMenuElement element in GetElements())
                element.Draw(spriteBatch, Bounds.Start + offset);
        }
    }


    public class MenuTextChoice : MenuChoice
    {
        public MenuText Text;

        public MenuTextChoice(string text, Action choiceAction) : this("", text, choiceAction, true, Color.White) { }

        public MenuTextChoice(string label, string text, Action choiceAction) : this(label, text, choiceAction, true, Color.White) { }

        public MenuTextChoice(string text, Action choiceAction, bool enabled, Color color) : this("", text, choiceAction, enabled, color) { }

        public MenuTextChoice(string label, string text, Action choiceAction, bool enabled, Color color)
            : base(label, choiceAction, enabled)
        {
            Text = new MenuText(text, new Loc(2, 1), color);
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return Text;
        }
    }

    public class MenuElementChoice : MenuChoice
    {
        public List<IMenuElement> Elements;

        public MenuElementChoice(Action choiceAction, bool enabled, params IMenuElement[] elements) : this("", choiceAction, enabled, elements) { }
        public MenuElementChoice(string label, Action choiceAction, bool enabled, params IMenuElement[] elements) : base(label, choiceAction, enabled)
        {
            ChoiceAction = choiceAction;
            Enabled = enabled;

            Elements = new List<IMenuElement>();
            foreach (IMenuElement element in elements)
                Elements.Add(element);
        }
        public override IEnumerable<IMenuElement> GetElements()
        {
            foreach (IMenuElement element in Elements)
                yield return element;
        }

        public int GetElementIndexByLabel(string label)
        {
            return GetElementIndicesByLabel(label)[label];
        }
        public virtual Dictionary<string, int> GetElementIndicesByLabel(params string[] labels)
        {
            return MenuBase.SearchLabels(labels, Elements);
        }
    }
}
