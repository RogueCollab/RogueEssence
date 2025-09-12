using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework.Graphics;

namespace RogueEssence.Menu
{
    public class WaitMenu : InteractableMenu
    {
        private readonly List<FrameInput.InputType> allowedInputs = [];

        public WaitMenu(bool anyInputs) : this(MenuLabel.WAIT, anyInputs ? [] : [FrameInput.InputType.Confirm]) { }
        public WaitMenu(params FrameInput.InputType[] inputs) : this(MenuLabel.WAIT, inputs) { }
        public WaitMenu(string label, bool anyInputs) : this(label, anyInputs ? [] : [FrameInput.InputType.Confirm]) { }
        public WaitMenu(string label, params FrameInput.InputType[] inputs)
        {
            Label = label;
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
