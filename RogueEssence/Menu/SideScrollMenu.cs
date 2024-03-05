using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class SideScrollMenu : InteractableMenu
    {
        protected const int CURSOR_FLASH_TIME = 24;

        public ulong PrevTick;
        public int arrowHeight;

        public void Initialize()
        {
            Initialize(GraphicsManager.ScreenHeight / 2);
        }
        public void Initialize(int arrowH)
        {
            PrevTick = GraphicsManager.TotalFrameTick % (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME);
            arrowHeight = arrowH;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);

            //draw cursor
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || Inactive)
            {
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.X - GraphicsManager.Cursor.TileWidth, arrowHeight), 0, 0, Color.White, SpriteEffects.FlipHorizontally);
                GraphicsManager.Cursor.DrawTile(spriteBatch, new Vector2(Bounds.End.X, arrowHeight), 0, 0);
            }

        }
    }
}
