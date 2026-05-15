using System;
using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class GroundEditorPageViewModel : EditorPageViewModel
{
    // public override string Title => "Ground Editor Long Name";
    
    public GroundEditorPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    { }
}