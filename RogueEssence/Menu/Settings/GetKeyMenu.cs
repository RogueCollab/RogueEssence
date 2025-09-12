using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RogueEssence.Menu
{
    public class GetKeyMenu : InteractableMenu
    {
        public delegate void OnChooseKey(Keys key);

        private OnChooseKey chooseKeyAction;
        private Action refuseAction;

        private HashSet<Keys> forbidden;

        public GetKeyMenu(HashSet<Keys> forbidden, OnChooseKey action, Action refuseAction) : this(MenuLabel.GET_KEY, forbidden, action, refuseAction) { }
        public GetKeyMenu(string label, HashSet<Keys> forbidden, OnChooseKey action, Action refuseAction)
        {
            Label = label;
            Bounds = new Rect();

            this.forbidden = forbidden;
            this.chooseKeyAction = action;
            this.refuseAction = refuseAction;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }

        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield break;
        }

        public override void Update(InputManager input)
        {
            Visible = true;

            bool pressed = false;
            for (int ii = 0; ii < 0xf5; ii++)
            {
                if (input.BaseKeyPressed((Keys)ii))
                {
                    if (forbidden.Contains((Keys)ii))
                        GameManager.Instance.SE("Menu/Cancel");
                    else
                    {
                        GameManager.Instance.SE("Menu/Confirm");
                        MenuManager.Instance.RemoveMenu();
                        chooseKeyAction((Keys)ii);
                    }
                    pressed = true;
                    break;
                }
            }
            if (!pressed && input.AnyButtonPressed())
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
                refuseAction();
            }
        }
    }
}
