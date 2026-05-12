using System;
using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class ZoneEditorPageViewModel : EditorPageViewModel
{
    // public override string Title => "Zone Editor";
    
    public ZoneEditorPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
        
    }
    
}
    