using System.Collections.Generic;
using RogueElements;
using System.Linq;

namespace RogueEssence.Menu
{
    public abstract class ChoiceMenu : InteractableMenu
    {
        public List<IMenuElement> NonChoices { get { return Elements; } }
        public List<IChoosable> Choices;

        protected MenuCursor cursor;

        public ChoiceMenu()
        {
            cursor = new MenuCursor(MenuLabel.CURSOR, this, Dir4.Right);
            Choices = new List<IChoosable>();
        }
        protected override IEnumerable<IMenuElement> GetDrawElements()
        {
            yield return cursor;
            foreach (IChoosable choice in Choices)
                yield return choice;
            foreach (IMenuElement nonChoice in NonChoices)
                yield return nonChoice;
        }


        public int GetChoiceIndexByLabel(string label)
        {
            return GetChoiceIndicesByLabel(label)[label];
        }
        public virtual Dictionary<string, int> GetChoiceIndicesByLabel(params string[] labels)
        {
            return SearchLabels(labels, Choices);
        }

        public int GetNonChoiceIndexByLabel(string label)
        {
            return GetNonChoiceIndicesByLabel(label)[label];
        }
        public virtual Dictionary<string, int> GetNonChoiceIndicesByLabel(params string[] labels)
        {
            return SearchLabels(labels, NonChoices);
        }
    }
}
