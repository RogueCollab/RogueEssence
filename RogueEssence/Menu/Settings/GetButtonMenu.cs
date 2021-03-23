using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RogueEssence.Menu
{
    public class GetButtonMenu : InteractableMenu
    {
        public delegate void OnChooseButton(Buttons button);

        private OnChooseButton chooseButtonAction;
        private Action refuseAction;

        private HashSet<Buttons> forbidden;

        public GetButtonMenu(HashSet<Buttons> forbidden, OnChooseButton action, Action refuseAction)
        {
            Bounds = new Rect();

            this.forbidden = forbidden;
            this.chooseButtonAction = action;
            this.refuseAction = refuseAction;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield break;
        }

        public override void Update(InputManager input)
        {
            Visible = true;

            bool pressed = false;
            foreach (Buttons button in Enum.GetValues(typeof(Buttons)))
            {
                if (input.BaseButtonPressed(button))
                {
                    if (forbidden.Contains(button))
                        GameManager.Instance.SE("Menu/Cancel");
                    else
                    {
                        GameManager.Instance.SE("Menu/Confirm");
                        MenuManager.Instance.RemoveMenu();
                        chooseButtonAction(button);
                    }
                    pressed = true;
                    break;
                }
            }
            if (!pressed && input.AnyKeyPressed())
            {
                GameManager.Instance.SE("Menu/Cancel");
                MenuManager.Instance.RemoveMenu();
                refuseAction();
            }
        }
    }
}
