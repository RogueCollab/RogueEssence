using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueEssence.Menu
{
    public class GetKeyMenu : InteractableMenu
    {
        public delegate void OnChooseKey(Keys key);

        private OnChooseKey chooseKeyAction;

        private HashSet<Keys> forbidden;

        public GetKeyMenu(HashSet<Keys> forbidden, OnChooseKey action)
        {
            Bounds = new Rect();

            this.forbidden = forbidden;
            this.chooseKeyAction = action;
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
                    break;
                }
            }
        }
    }
}
