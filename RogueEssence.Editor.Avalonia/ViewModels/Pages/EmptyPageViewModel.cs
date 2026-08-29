using System;

namespace RogueEssence.Dev.ViewModels;

public class EmptyPageViewModel : EditorPageViewModel
{
    public override bool AddNewTab => false;
    
    public EmptyPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
    }
    
}