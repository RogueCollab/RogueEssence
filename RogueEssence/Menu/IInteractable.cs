using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public interface IInteractable : ILabeled
    {
        bool IsCheckpoint { get; }
        bool Visible { get; set; }
        bool Inactive { get; set; }
        bool BlockPrevious { get; set; }

        void Update(InputManager input);
        void ProcessActions(FrameTick elapsedTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
