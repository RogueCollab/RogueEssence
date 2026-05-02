using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class RandomInfoPageViewModel : EditorPageViewModel
{
    public override bool AddNewTab => false;
    
    // public override string Title => "Random Page Info";
    public RandomInfoPageViewModel(NodeFactory nodeFactory, PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService,
        NodeBase node) : base(nodeFactory, pageFactory, tabEvents, dialogService)
    {
    }
    
}