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
            if(GetChoiceIndexesByLabel(label).TryGetValue(label, out int ret)) return ret;
            return -1;
        }
        public virtual Dictionary<string, int> GetChoiceIndexesByLabel(params string[] labels)
        {
            Dictionary<string, int> poss = new();
            List<string> labelList = labels.ToList();
            foreach (string label in labels)
                poss.Add(label, -1);

            for (int ii = 0; ii < Choices.Count; ii++)
            {
                bool found = false;
                IChoosable choice = Choices[ii];
                if (choice.HasLabel())
                {
                    for (int kk = 0; kk < labelList.Count; kk++)
                    {
                        string label = labelList[kk];
                        if (choice.Label == label)
                        {
                            found = true;
                            poss[label] = ii;
                            labelList.RemoveAt(kk);
                            break;
                        }
                    }
                }
                if (found && labelList.Count == 0) break;
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
