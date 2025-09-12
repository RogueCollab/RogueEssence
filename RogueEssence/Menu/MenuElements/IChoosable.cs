using RogueElements;

namespace RogueEssence.Menu
{
    public interface IChoosable : IMenuElement
    {
        Rect Bounds { get; set; }
        bool Selected { get; }
        bool Hovered { get; }

        //chosen by clicking
        void OnMouseState(bool clicked);
        
        void OnSelect(bool select);

        //selected by mouse
        void OnHoverChanged(bool hover);

        //chosen by confirm button
        void OnConfirm();
    }
}
