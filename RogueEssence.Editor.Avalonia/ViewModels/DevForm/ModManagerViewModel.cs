using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using RogueEssence;
using RogueEssence.Script;
using System.Collections.ObjectModel;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RogueEssence.Data;
using RogueEssence.Content;
using RogueEssence.Dev.Views;
using Avalonia.Controls;
using RogueEssence.Dev;



namespace RogueEssence.Dev.ViewModels
{
    
    public class ModsEntryViewModel : ViewModelBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _editNamespace;

        public string Namespace
        {
            get => _editNamespace;
            set => this.RaiseAndSetIfChanged(ref _editNamespace, value);
        }

        public string Path;

        public ModsEntryViewModel(string name, string newNamespace, string fullPath)
        {
            this._name = name;
            this._editNamespace = newNamespace;
            this.Path = fullPath;
        }
    
        public string Display => $"{_editNamespace}: {_name}";
    }
    
    public class ModManagerViewModel : ViewModelBase
    {
        private EditorContext _context;
        public ModManagerViewModel(EditorContext context)
        {
            currentMod = null;
            _context = context;

            ModsList = new ObservableCollection<ModsEntryViewModel>();
            ReloadMods();
        }
        
        
        private string currentModString;

        public string CurrentModString
        {
            get => currentModString;
            set => this.SetIfChanged(ref currentModString, value);
        }

        public void UpdateMod()
        {
            CurrentModString = _getModName(PathMod.Quest);
        }
    
        private static string _getModName(ModHeader mod)
        {
            if (!mod.IsValid())
                return "Origin";
            return mod.GetMenuName();
        }
        
        

        private ModsEntryViewModel currentMod;
        public ModsEntryViewModel CurrentMod
        {
            get => currentMod;
            set
            {
                this.SetIfChanged(ref currentMod, value);
            }
        }

        public ObservableCollection<ModsEntryViewModel> ModsList { get; }
        
        public async Task AskSwitchTo(ModsEntryViewModel mod)
        {
            
            //give a pop up warning that the game will be reloaded and wait for confirmation
            MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_context.DialogService, $"The game will be reloaded to use content from {mod.Name}.\nClick OK to proceed.", "Are you sure?",
                MessageBoxWindowView.MessageBoxButtons.OkCancel);
            if (result == MessageBoxWindowView.MessageBoxResult.Cancel)
                return;

            DevForm.ExecuteOrPend(() => _switchTo(mod));
        }
        
        public async Task<bool> CheckIsCurrentMod(ModsEntryViewModel mod)
        {
            if (mod.Namespace == CurrentMod.Namespace)
            {
                await MessageBoxWindowView.Show(_context.DialogService, $"{CurrentMod.Name} is already currently active", "Switch Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                return true;
            }

            return false;
        }
        
        private void _switchTo(ModsEntryViewModel mod)
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
                DiagManager.Instance.DevEditor.MapEditor = null;
                DiagManager.Instance.DevEditor.GroundEditor = null;
            }
      
        }

        public void ReloadMods()
        {
            ModsList.Clear();
            ModsList.Add(new ModsEntryViewModel("Origin", PathMod.BaseNamespace, ""));
            string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
            ModsEntryViewModel chosenModel = null;
            foreach (string modPath in modsPath)
            {
                ModHeader header = PathMod.GetModDetails(modPath);
                ModsList.Add(new ModsEntryViewModel(getModName(header), header.Namespace, Path.Combine(PathMod.MODS_FOLDER, Path.GetFileName(modPath))));
                if (PathMod.Quest.Path == header.Path)
                    chosenModel = ModsList[ModsList.Count - 1];
            }

 
            CurrentMod = chosenModel;
        }

        public void RemoveMod(ModsEntryViewModel mod)
        {
            ModsList.Remove(mod);
        }
        
        private static string getModName(ModHeader mod)
        {
            if (!mod.IsValid())
                return null;
            return mod.GetMenuName();
        }
    }
}
