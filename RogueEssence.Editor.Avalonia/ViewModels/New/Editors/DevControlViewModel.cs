using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class DevControlViewModel : EditorPageViewModel
{
    public override string UniqueId => "DevControl";
    public override string? Title => "Dev Control";

    public TestComboBoxViewModel Fruits { get; }
    
    public DevControlViewModel (PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService) : base(pageFactory, tabEvents, dialogService)
    {
       
        Fruits = new TestComboBoxViewModel();
    }
    
    public DevControlViewModel() : base(new PageFactory(new DesignServiceProvider()), new TabEvents(new PageFactory(new DesignServiceProvider())), new DialogService())
    {
        Title = "Dev Control";
        Fruits = new TestComboBoxViewModel();
    }
    
}