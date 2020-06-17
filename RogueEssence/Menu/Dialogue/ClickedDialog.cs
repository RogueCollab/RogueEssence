using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{

    public class ClickedDialog : DialogueBox
    {
        private Action action;

        public ClickedDialog(string message, bool sound, Action action)
            : base(message, sound)
        {
            this.action = action;
        }

        public override void ProcessTextDone(InputManager input)
        {
            //needs to update the cursor's flashing?
            //do we need this cursor to be separate from the mid-message pauses?

            if (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel])
            {
                //close this
                MenuManager.Instance.RemoveMenu();

                //do what it wants
                action();
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw down-tick
            if (Text.Finished && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(GraphicsManager.ScreenWidth / 2 - 5, Bounds.End.Y - 6), 1, 0);
        }
    }
}
