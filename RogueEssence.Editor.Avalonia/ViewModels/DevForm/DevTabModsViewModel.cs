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
using RogueEssence.Data;
using RogueEssence.Content;
using RogueEssence.Dev.Views;
using Avalonia.Controls;

namespace RogueEssence.Dev.ViewModels
{
    public class ModsNodeViewModel : ViewModelBase
    {
        private string name;
        public string Name
        {
            get => name;
            set => this.SetIfChanged(ref name, value);
        }

        public string FullPath;
        public ModsNodeViewModel Parent;

        public ObservableCollection<ModsNodeViewModel> Nodes { get; }

        public ModsNodeViewModel(ModsNodeViewModel parent, string name, string fullPath)
        {
            this.Parent = parent;
            this.name = name;
            this.FullPath = fullPath;
            Nodes = new ObservableCollection<ModsNodeViewModel>();
        }

    }

    public class DevTabModsViewModel : ViewModelBase
    {
        public DevTabModsViewModel()
        {
            currentMod = "";

            Mods = new ObservableCollection<ModsNodeViewModel>();
            ModsNodeViewModel baseNode = new ModsNodeViewModel(null, getModName(""), "");
            string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
            foreach (string modPath in modsPath)
                baseNode.Nodes.Add(new ModsNodeViewModel(baseNode, getModName(modPath), Path.Combine(PathMod.MODS_FOLDER, getModName(modPath))));
            Mods.Add(baseNode);
        }

        private string currentMod;
        public string CurrentMod
        {
            get => currentMod;
            set => this.SetIfChanged(ref currentMod, value);
        }

        private ModsNodeViewModel chosenMod;
        public ModsNodeViewModel ChosenMod
        {
            get => chosenMod;
            set => this.SetIfChanged(ref chosenMod, value);
        }

        public ObservableCollection<ModsNodeViewModel> Mods { get; }

        public void UpdateMod()
        {
            CurrentMod = "Current Mod: " + getModName(PathMod.Mod);
        }

        public async void btnSwitch_Click()
        {
            //check to be sure it isn't the current mod
            if (chosenMod.FullPath == PathMod.Mod)
                return;

            //give a pop up warning that the game will be reloaded and wait for confirmation
            MessageBox.MessageBoxResult result = await MessageBox.Show((Window)DiagManager.Instance.DevEditor, "The game will be reloaded to use content from the new path.\nClick OK to proceed.", "Are you sure?",
                MessageBox.MessageBoxButtons.OkCancel);
            if (result == MessageBox.MessageBoxResult.Cancel)
                return;

            //modify and reload
            lock (GameBase.lockObj)
            {
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SceneOutcome = GameManager.Instance.SetMod(chosenMod.FullPath, false);
            }
        }

        public async void btnAdd_Click()
        {
            //pop up a name input
            RenameViewModel vm = new RenameViewModel();
            RenameWindow window = new RenameWindow()
            {
                DataContext = vm
            };

            bool result = await window.ShowDialog<bool>((DevForm)DiagManager.Instance.DevEditor);
            if (!result)
                return;
            
            string newName = Regex.Replace(vm.Name, "\\W", "_");

            //sanitize name and check for name conflicts
            if (newName == "")
                return;

            ModsNodeViewModel chosenNode = Mods[0];// ChosenMod;
            //check for children name conflicts
            foreach (ModsNodeViewModel child in chosenNode.Nodes)
            {
                if (String.Equals(child.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    //already exists, pop up message
                    await MessageBox.Show((Window)DiagManager.Instance.DevEditor, newName + " already exists!", "Add Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
            }

            ModsNodeViewModel newNode = new ModsNodeViewModel(chosenNode, newName, Path.Combine(chosenNode.FullPath, PathMod.MODS_FOLDER, newName));
            //add all asset folders
            Directory.CreateDirectory(newNode.FullPath);
            //add Strings
            Directory.CreateDirectory(Path.Join(newNode.FullPath, "Strings"));
            //Content
            GraphicsManager.InitContentFolders(newNode.FullPath);
            //Data
            DataManager.InitDataDirs(newNode.FullPath);
            //Script
            LuaEngine.InitScriptFolders(newNode.FullPath);

            //add node
            chosenNode.Nodes.Add(newNode);
        }

        public async void btnDelete_Click()
        {
            //prohibit the deletion of the current node or the base node
            if (chosenMod == Mods[0])
            {
                await MessageBox.Show((Window)DiagManager.Instance.DevEditor, "Cannot delete the root mod!", "Delete Failed", MessageBox.MessageBoxButtons.Ok);
                return;
            }
            //ask for confirmation
            MessageBox.MessageBoxResult result = await MessageBox.Show((Window)DiagManager.Instance.DevEditor, "Are you sure you want to delete the mod in directory:\n" + chosenMod.FullPath, "Are you sure?",
                MessageBox.MessageBoxButtons.YesNo);
            if (result == MessageBox.MessageBoxResult.No)
                return;

            //delete folder
            Directory.Delete(chosenMod.FullPath, true);

            //and then delete node
            chosenMod.Parent.Nodes.Remove(chosenMod);
        }

        private static string getModName(string path)
        {
            if (path == "")
                return "[None]";

            //TODO: allow for multi-tiered mods
            return Path.GetFileName(path);
        }
    }
}
