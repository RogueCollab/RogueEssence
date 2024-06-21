using System.Collections.Generic;
using RogueElements;
using System.Linq;

namespace RogueEssence.Menu
{
    public abstract class ChoiceMenu : InteractableMenu
    {
        public override List<IMenuElement> Elements {
            get => NonChoices.Concat(Choices).ToList();
            protected set { NonChoices = value; Choices = new(); } //Realistically only affects initialization
        }
        public List<IMenuElement> NonChoices;
        public List<IChoosable> Choices;

        protected MenuCursor cursor;

        public ChoiceMenu()
        {
            cursor = new MenuCursor(MenuLabel.CURSOR, this, Dir4.Right);
            NonChoices = new List<IMenuElement> {cursor};
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
    }
}
