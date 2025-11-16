using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class ZoneEditorPageViewModel : EditorPageViewModel
{
    public override string UniqueId => "ZoneEditor";
    public override string Title => "Zone Editor";
    
    public ZoneEditorPageViewModel(PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService) : base(pageFactory, tabEvents, dialogService)
    {
        
    }
    
}
    