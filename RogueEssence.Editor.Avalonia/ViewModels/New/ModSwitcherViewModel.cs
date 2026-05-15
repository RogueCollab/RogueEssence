using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Dev.Views;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels;

public class ModSwitcherViewModel : ViewModelBase
{

    private readonly IDialogService _dialogService;
    private readonly DevFormViewModel _mainWindow;
    private ModsNodeViewModel _selectedMod;
    
    public ModsNodeViewModel SelectedMod
    {
        get => _selectedMod;
        set { this.RaiseAndSetIfChanged(ref _selectedMod, value); }
    }
    
    public ObservableCollection<ModsNodeViewModel> Mods { get; }
    public ModSwitcherViewModel(DevFormViewModel mainWindowViewModel, IDialogService dialogService)
    {
        _mainWindow = mainWindowViewModel;
        _dialogService = dialogService;

        Mods = new ObservableCollection<ModsNodeViewModel>(mainWindowViewModel.Mods);
    }
    
    
    public void CloseSwitcher()
    {
        _mainWindow.OnModSwitcherClosed();
    }
    
    public async Task ConfirmModSwitchAsync()
    {
        var res = await MessageBoxWindowView.Show(_dialogService, "The game will be reloaded to use content from the new path. Click OK to proceed. Your changes will not be saved.", "Are you sure?", MessageBoxWindowView.MessageBoxButtons.OkCancel);
        if (res == MessageBoxWindowView.MessageBoxResult.Ok)
        {
            DevForm.ExecuteOrPend(() => _doSwitch(SelectedMod));
        }
    }
    
    public ModsNodeViewModel CurrentMod
    {
        get => _mainWindow.ChosenMod;
        set => _mainWindow.ChosenMod = value;
    }

    public bool IsCurrent(ModsNodeViewModel head)
    { 
        Console.WriteLine("0" + _mainWindow.ChosenMod);
        Console.WriteLine("1" + head.Namespace);
        return head.Namespace == _mainWindow.ChosenMod.Namespace;
    }
    
    private void _doSwitch(ModsNodeViewModel mod)
    {
        //modify and reload
        lock (GameBase.lockObj)
        {
            LuaEngine.Instance.BreakScripts();
            MenuManager.Instance.ClearMenus();
            if (!String.IsNullOrEmpty(mod.Path))
                GameManager.Instance.SetQuest(PathMod.GetModDetails(PathMod.FromApp(mod.Path)), new ModHeader[0] { }, new List<int>() { -1 });
            else
                GameManager.Instance.SetQuest(ModHeader.Invalid, new ModHeader[0] { }, new List<int>() { });

            DiagManager.Instance.PrintModSettings();
            DiagManager.Instance.SaveModSettings();
        }
    }
}


