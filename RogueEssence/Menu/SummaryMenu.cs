using System.Collections.Generic;
using RogueElements;

namespace RogueEssence.Menu
{
    public class SummaryMenu : MenuBase
    {
        public List<IMenuElement> Elements;

        public SummaryMenu(Rect bounds)
        {
            Bounds = bounds;
            Elements = new List<IMenuElement>();
        }

        public override IEnumerable<IMenuElement> GetElements()
        {
            foreach (IMenuElement element in Elements)
                yield return element;
        }
    }
}
