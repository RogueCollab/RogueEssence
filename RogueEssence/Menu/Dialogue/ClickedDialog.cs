using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueElements;
using RogueEssence.Content;

namespace RogueEssence.Menu
{

    public class ClickedDialog : DialogueBox
    {
        private Action action;

        public ClickedDialog(string message, bool sound, bool centerH, bool centerV, Rect bounds, Action action)
            : base(message, sound, centerH, centerV, bounds)
        {
            this.action = action;
        }

        public override void ProcessTextDone(InputManager input)
        {
            //needs to update the cursor's flashing?
            //do we need this cursor to be separate from the mid-message pauses?

            if (input.JustPressed(FrameInput.InputType.Confirm) || input[FrameInput.InputType.Cancel] && TotalTextTime >= HOLD_CANCEL_TIME
                || input.JustPressed(FrameInput.InputType.LeftMouse))
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

            TextPause textPause = getCurrentTextPause();
            //draw down-tick
            if (Finished && textPause == null && (GraphicsManager.TotalFrameTick / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0)
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.Center.X - 5, Bounds.End.Y - 6), 1, 0);
        }
    }
}
