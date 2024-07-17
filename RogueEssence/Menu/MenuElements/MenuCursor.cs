using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public class MenuCursor : IMenuElement
    {
        public string Label { get; set; }
        protected const int CURSOR_FLASH_TIME = 24;

        public ulong PrevTick;

        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
        public bool LabelContains(string substr)
        {
            return HasLabel() && Label.Contains(substr);
        }

        public Loc Loc { get; set; }

        public Dir4 Direction;

        private IInteractable baseMenu;
        public MenuCursor(IInteractable baseMenu) : this("", baseMenu, Dir4.Right) { }
        public MenuCursor(string label, IInteractable baseMenu) : this(label, baseMenu, Dir4.Right) { }
        public MenuCursor(IInteractable baseMenu, Dir4 dir) : this("", baseMenu, dir) { }
        public MenuCursor(string label, IInteractable baseMenu, Dir4 dir)
        {
            this.Label = label;
            this.baseMenu = baseMenu;
            this.Direction = dir;
        }

        public void ResetTimeOffset()
        {
            PrevTick = GraphicsManager.TotalFrameTick % (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME);
        }

        public void Draw(SpriteBatch spriteBatch, Loc offset)
        {
            //draw cursor
            if (((GraphicsManager.TotalFrameTick - PrevTick) / (ulong)FrameTick.FrameToTick(CURSOR_FLASH_TIME / 2)) % 2 == 0 || baseMenu.Inactive)
            {
                switch (Direction)
                {
                    case Dir4.Down:
                        GraphicsManager.Cursor.DrawTile(spriteBatch, (Loc + offset).ToVector2(), 1, 0);
                        break;
                    case Dir4.Left:
                        GraphicsManager.Cursor.DrawTile(spriteBatch, (Loc + offset).ToVector2(), 0, 0, Color.White, SpriteEffects.FlipHorizontally);
                        break;
                    case Dir4.Up:
                        GraphicsManager.Cursor.DrawTile(spriteBatch, (Loc + offset).ToVector2(), 1, 0, Color.White, SpriteEffects.FlipVertically);
                        break;
                    case Dir4.Right:
                        GraphicsManager.Cursor.DrawTile(spriteBatch, (Loc + offset).ToVector2(), 0, 0);
                        break;
                }
            }
        }
    }
}
