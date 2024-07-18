using System.Collections.Generic;
using RogueElements;
using System.Linq;

namespace RogueEssence.Menu
{
    public abstract class ChoiceMenu : InteractableMenu
    {
        public override List<IMenuElement> Elements {
            get
            {
                return NonChoices.Concat(Choices).ToList();
            }
            protected set
            {
                //Realistically only affects initialization
                NonChoices = value;
                Choices = new List<IChoosable>();
            }
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

        public override Dictionary<string, int> GetElementIndicesByLabel(params string[] labels)
        {
            return SearchLabels(labels, Elements);
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
