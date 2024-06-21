using RogueElements;

namespace RogueEssence.Menu
{
    public class SummaryMenu : MenuBase
    {
        public SummaryMenu(string label, Rect bounds) : this(bounds) { Label = label; }
        public SummaryMenu(Rect bounds) { Bounds = bounds; }
    }
}
