using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;

namespace RogueEssence.Menu
{
    public abstract class ChoiceMenu : InteractableMenu
    {
        public List<IMenuElement> NonChoices;
        public List<IChoosable> Choices;

        //TODO: add this into the non-choices list?
        protected MenuCursor cursor;

        public ChoiceMenu()
        {
            cursor = new MenuCursor(this, Dir4.Right);
            NonChoices = new List<IMenuElement>();
            Choices = new List<IChoosable>();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            yield return cursor;
            foreach (IChoosable choice in Choices)
                yield return choice;
            foreach (IMenuElement nonChoice in NonChoices)
                yield return nonChoice;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
                return;
            base.Draw(spriteBatch);
        }
    }
}
