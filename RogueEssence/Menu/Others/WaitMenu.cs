using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Menu
{
    public class WaitMenu : InteractableMenu
    {
        private readonly List<FrameInput.InputType> allowedInputs = [];

        public WaitMenu(bool anyInputs) : this(anyInputs ? [] : [FrameInput.InputType.Confirm]) { }
        public WaitMenu(params FrameInput.InputType[] inputs)
        {
            Bounds = new Rect();

            foreach (var input in inputs) {
                allowedInputs.Add(input);
            }
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

            if (allowedInputs.Count > 0)
            {
                foreach (FrameInput.InputType inputType in allowedInputs)
                {
                    if (input.JustPressed(inputType))
                        MenuManager.Instance.RemoveMenu();
                }
            }
            else
            {
                if (input.AnyButtonPressed() || input.AnyKeyPressed())
                    MenuManager.Instance.RemoveMenu();
            }
        }
    }
}
