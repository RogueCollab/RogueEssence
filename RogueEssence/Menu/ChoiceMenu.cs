using System.Collections.Generic;
using RogueElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueEssence.Content;
using System.Linq;

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

        public virtual int GetChoiceIndexByLabel(string label)
        {
            return GetChoiceIndexesByLabel(label)[label];
        }
        public virtual Dictionary<string, int> GetChoiceIndexesByLabel(params string[] labels)
        {
            Dictionary<string, int> poss = new Dictionary<string, int>();
            int totalFound = 0;
            foreach (string label in labels)
                poss.Add(label, -1);

            for (int ii = 0; ii < Choices.Count; ii++)
            {
                IChoosable choice = Choices[ii];
                int curIndex;
                if (choice.HasLabel() && poss.TryGetValue(choice.Label, out curIndex))
                {
                    // case for duplicate labels somehow; only get the first index found
                    if (curIndex == -1)
                    {
                        poss[choice.Label] = ii;
                        totalFound++;

                        // short-circuit case for having found all indices
                        if (totalFound == poss.Count)
                            return poss;
                    }
                }
            }
            return poss;
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
