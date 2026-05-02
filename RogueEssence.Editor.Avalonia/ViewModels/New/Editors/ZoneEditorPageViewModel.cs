using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class ZoneEditorPageViewModel : EditorPageViewModel
{
    // public override string Title => "Zone Editor";
    
    public ZoneEditorPageViewModel(NodeFactory nodeFactory, PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService,
        NodeBase node) : base(nodeFactory, pageFactory, tabEvents, dialogService)
    {
        
    }
    
}
    