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

        public override Dictionary<string, LabeledElementIndex> GetElementIndexesByLabel(params string[] labels)
            => SearchLabels(labels, UnboundElements, NonChoices, Choices);

        public LabeledElementIndex GetChoiceIndexByLabel(string label)
        {
            if (GetElementIndexesByLabel(label).TryGetValue(label, out LabeledElementIndex ret)) return ret;
            return new LabeledElementIndex();
        }
        public virtual Dictionary<string, LabeledElementIndex> GetChoiceIndexesByLabel(params string[] labels)
            => SearchLabels(labels, Choices);
    }
}
