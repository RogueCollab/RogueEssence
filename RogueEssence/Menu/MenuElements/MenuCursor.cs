using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuCursor : IMenuElement
    {
        protected const int CURSOR_FLASH_TIME = 24;

        public ulong PrevTick;

        public Loc Loc { get; set; }

        private IInteractable baseMenu;
        public MenuCursor(IInteractable baseMenu)
        {
            this.baseMenu = baseMenu;
        }

        public void ResetTimeOffset()
        {
            PrevTick = GraphicsManager.TotalFrameTick % (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME);
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            //draw cursor
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || baseMenu.Inactive)
                GraphicsManager.Cursor.DrawTile(spriteBatch, (Loc + offset).ToVector2(), 0, 0);

        }
    }
}
