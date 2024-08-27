using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RogueEssence.Menu
{
    public class WaitMenu : InteractableMenu
    {
        private bool anyKey;

        public WaitMenu(bool anyInput)
        {
            Bounds = new Rect();

            this.anyKey = anyInput;
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

            if (input.JustPressed(FrameInput.InputType.Confirm) || anyKey && (input.AnyButtonPressed() || input.AnyKeyPressed()))
                MenuManager.Instance.RemoveMenu();
        }
    }
}
