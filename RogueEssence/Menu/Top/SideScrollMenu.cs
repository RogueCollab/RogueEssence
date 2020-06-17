using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class SideScrollMenu : InteractableMenu
    {
        protected const int CURSOR_FLASH_TIME = 24;

        public ulong PrevTick;

        public void Initialize()
        {
            PrevTick = GraphicsManager.TotalFrameTick % (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw cursor
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || Inactive)
            {
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.X - 11, GraphicsManager.ScreenHeight / 2), 0, 0, Color.White, SpriteEffects.FlipHorizontally);
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.End.X, GraphicsManager.ScreenHeight / 2), 0, 0);
            }

        }
    }
}
