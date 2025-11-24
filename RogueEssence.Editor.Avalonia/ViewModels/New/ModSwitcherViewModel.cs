using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

public class ModSwitcherViewModel : ViewModelBase
{

    private readonly IDialogService _dialogService;
    private readonly DevFormViewModel _mainWindow;
    private ModHeader _selectedMod;

    public ModHeader SelectedMod
    {
        get => _selectedMod;
        set { this.RaiseAndSetIfChanged(ref _selectedMod, value); }
    }
    
    public ObservableCollection<Models.ModHeader> Mods { get; }
    public ModSwitcherViewModel(DevFormViewModel mainWindowViewModel, IDialogService dialogService)
    {
        _mainWindow = mainWindowViewModel;
        _dialogService = dialogService;
        
        Mods = new ObservableCollection<Models.ModHeader>()
        {
            // new Models.ModHeader("[NULL]", "origin"),
            // new ModHead("Super Party Bros", "Super_Party_Bros"),
            // new ModHead("Resource Dungeon Pack", "source_duns_imbi"),
            // new ModHead("Trio's Sandbox", "trios_sandbox"),
            // new ModHead("Bubsy Mystery Dungeon", "bubsy_md"),
            // new ModHead("Friend Area", "friend_area"),
            // new ModHead("Typeless Moves", "typeless_moves"),
            // new ModHead("Bossfights", "bossfights"),
            // new ModHead("Explorers of Friending", "explorers_of_friending"),
            // new ModHead("All Starters", "starter_mod"),
            // new ModHead("Visible Monster Houses", "visible_monster_houses"),
            // new ModHead("Project EoN (0.2.3)", "what_does_n_stand_for"),
            // new ModHead("Explorers of Kanto", "explorers_of_kanto"),
            // new ModHead("Halcyon", "halcyon"),
            // new ModHead("Ruin", "ruins"),
            // new ModHead("More Items", "more_items"),
            // new ModHead("Enable Mission Board", "enable_mission_board"),
            // new ModHead("Silver Resistance", "silver_resistance"),
            // new ModHead("Rev Mod", "rev_mod"),
            // new ModHead("Halcyon No Nickname", "halcyon_nickname"),
            // new ModHead("Mystery Ruins", "mystery_ruins"),
            // new ModHead("Trio's Sandbox", "trios_sandbox_pack"),
            // new ModHead("Music Notice", "music_notice"),
            // new ModHead("Trio's Dungeon Pack", "trios_dungeon_pack"),
            // new ModHead("Raw", "raw_key"),
            // new ModHead("Gender Unlock", "gender_unlock"),

        };
    }
    
    
    public void CloseSwitcher()
    {
        _mainWindow.OnModSwitcherClosed();
    }
    
    public async Task<MessageBoxWindowView.MessageBoxResult> ConfirmModSwitchAsync()
    {
        return await MessageBoxWindowView.Show(_dialogService, "The game will be reloaded to use content from the new path. Click OK to proceed. Your changes will not be saved.", "Are you sure?", MessageBoxWindowView.MessageBoxButtons.OkCancel);
    }
    
    public Models.ModHeader CurrentMod
    {
        get => _mainWindow.CurrentMod;
        set => _mainWindow.CurrentMod = value;
    }

    public bool IsCurrent(Models.ModHeader head)
    { 
        return head.Key == _mainWindow.CurrentMod.Key;
    }
}


