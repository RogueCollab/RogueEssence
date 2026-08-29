using System;
using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class RandomInfoPageViewModel : EditorPageViewModel
{
    public override bool AddNewTab => false;
    
    // public override string Title => "Random Page Info";
    public RandomInfoPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
    }
    
}