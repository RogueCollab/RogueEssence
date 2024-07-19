using Microsoft.Xna.Framework.Graphics;
using RogueElements;

namespace RogueEssence.Menu
{
    public abstract class BaseMenuElement : IMenuElement
    {
        public string Label { get; set; }

        public bool HasLabel() => !string.IsNullOrEmpty(Label);
        public bool LabelContains(string substr) => HasLabel() && Label.Contains(substr);

        public abstract void Draw(SpriteBatch spriteBatch, Loc offset);
    }
}
