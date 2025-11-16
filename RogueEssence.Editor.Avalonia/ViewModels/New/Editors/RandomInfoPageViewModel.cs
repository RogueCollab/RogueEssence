using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class RandomInfoPageViewModel : EditorPageViewModel
{
    public override bool AddNewTab => false;
    
    public override string Title => "Random Page Info";
    public RandomInfoPageViewModel (PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService) : base(pageFactory, tabEvents, dialogService)
    {
    }
    
}