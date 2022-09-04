using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using NLua;

namespace RogueEssence.Menu
{
    public class ScriptableMenu : InteractableMenu
    {
        public List<IMenuElement> MenuElements;

        protected LuaFunction UpdateFunction;

        public ScriptableMenu(int x, int y, int w, int h, LuaFunction updateFunction)
        {
            UpdateFunction = updateFunction;
            Bounds = new Rect(x, y, w, h);
            MenuElements = new List<IMenuElement>();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            foreach (IMenuElement choice in MenuElements)
                yield return choice;
        }

        public override void Update(InputManager input)
        {
            UpdateFunction?.Call(input);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);
        }
    }
}
