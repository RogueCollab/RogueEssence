using Microsoft.Xna.Framework.Graphics;
using RogueElements;

namespace RogueEssence.Menu
{
    public interface IMenuElement
    {
        void Draw(SpriteBatch spriteBatch, Loc offset);
    }
}
