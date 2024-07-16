using Microsoft.Xna.Framework.Graphics;
using RogueElements;

namespace RogueEssence.Menu
{
    public interface IMenuElement : ILabeled
    {
        void Draw(SpriteBatch spriteBatch, Loc offset);
    }
}
