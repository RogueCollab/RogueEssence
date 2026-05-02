using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class DevControlViewModel : EditorPageViewModel
{
    public DevTabGameViewModel Game { get; }
    public DevTabPlayerViewModel Player { get; }

    public DevTabTravelViewModel Travel { get; }

    public DevTabScriptViewModel Script { get; }

    public override string DefaultTitle => "Dev Control";

    public TestComboBoxViewModel Fruits { get; }

    public DevControlViewModel(NodeFactory nodeFactory, PageFactory pageFactory, TabEvents tabEvents, IDialogService dialogService,
        DevTabGameViewModel game, DevTabPlayerViewModel player, DevTabTravelViewModel travel,
        DevTabScriptViewModel script, NodeBase node) : base(nodeFactory, pageFactory, tabEvents, dialogService)
    {
        Fruits = new TestComboBoxViewModel();
        Game = game;
        Player = player;
        Travel = travel;
        Script = script;
    }

    // public DevControlViewModel() : base(new PageFactory(new DesignServiceProvider()),
    //     new TabEvents(new PageFactory(new DesignServiceProvider())), new DialogService())
    // {
    //     // Title = "Dev Control";
    //     Fruits = new TestComboBoxViewModel();
    // }
}