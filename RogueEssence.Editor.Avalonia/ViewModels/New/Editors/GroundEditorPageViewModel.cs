using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class GroundEditorPageViewModel : EditorPageViewModel
{
    // public override string Title => "Ground Editor Long Name";
    
    public GroundEditorPageViewModel (PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService) : base(pageFactory, tabEvents, dialogService)
    { }
}