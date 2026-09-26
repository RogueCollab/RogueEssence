using System;
using RogueEssence.Dev.Services;

namespace RogueEssence.Dev.ViewModels;

public class DevControlPageViewModel : EditorPageViewModel
{
    public DevTabGameViewModel Game { get; }
    public DevTabPlayerViewModel Player { get; }

    public DevTabTravelViewModel Travel { get; }

    public DevTabScriptViewModel Script { get; }

    public override string DefaultTitle => "Dev Control";
    

    public DevControlPageViewModel(EditorContext context,
        DevTabGameViewModel game, DevTabPlayerViewModel player, DevTabTravelViewModel travel,
        DevTabScriptViewModel script, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
        Game = game;
        Player = player;
        Travel = travel;
        Script = script;
    }
}