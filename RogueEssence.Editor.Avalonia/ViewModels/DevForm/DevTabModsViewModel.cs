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
            currentMod = null;

            Mods = new ObservableCollection<ModsNodeViewModel>();
            reloadMods();
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
            CurrentMod = getModName(PathMod.Quest);
        }

        public async void btnSwitch_Click()
        {
            //check to be sure it isn't the current mod
            if (chosenMod.FullPath == PathMod.Quest.Path)
                return;

            //give a pop up warning that the game will be reloaded and wait for confirmation
            MessageBox.MessageBoxResult result = await MessageBox.Show((Window)DiagManager.Instance.DevEditor, "The game will be reloaded to use content from the new path.\nClick OK to proceed.", "Are you sure?",
                MessageBox.MessageBoxButtons.OkCancel);
            if (result == MessageBox.MessageBoxResult.Cancel)
                return;

            //modify and reload
            lock (GameBase.lockObj)
            {
                LuaEngine.Instance.BreakScripts();
                MenuManager.Instance.ClearMenus();
                GameManager.Instance.SetQuest(PathMod.GetModDetails(chosenMod.FullPath), new ModHeader[0] { });
            }
        }

        public async void btnAdd_Click()
        {
            ModConfigWindow window = new ModConfigWindow();
            ModHeader header = new ModHeader("", "", Guid.NewGuid(), new Version(), PathMod.ModType.Mod);
            ModConfigViewModel vm = new ModConfigViewModel(header);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            bool result = await window.ShowDialog<bool>(form);
            if (!result)
                return;
            
            string newName = Text.Sanitize(vm.Name);

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

            ModsNodeViewModel newNode = new ModsNodeViewModel(chosenNode, newName, Path.Combine(PathMod.MODS_PATH, newName));
            //add all asset folders
            Directory.CreateDirectory(newNode.FullPath);
            //create the mod xml
            ModHeader newHeader = new ModHeader(newNode.FullPath, vm.Name.Trim(), Guid.Parse(vm.UUID), Version.Parse(vm.Version), (PathMod.ModType)vm.ChosenModType);
            PathMod.SaveModDetails(newNode.FullPath, newHeader);

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

        public async void btnEdit_Click()
        {
            ModConfigWindow window = new ModConfigWindow();
            ModHeader header = PathMod.Quest;
            ModConfigViewModel viewModel = new ModConfigViewModel(header);
            window.DataContext = viewModel;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            bool result = await window.ShowDialog<bool>(form);

            if (result)
            {
                //save the mod data
                ModHeader resultHeader = new ModHeader(PathMod.Quest.Path, viewModel.Name.Trim(), Guid.Parse(viewModel.UUID), Version.Parse(viewModel.Version), (PathMod.ModType)viewModel.ChosenModType);
                PathMod.SaveModDetails(PathMod.Quest.Path, resultHeader);

                reloadMods();
            }
        }

        private void reloadMods()
        {
            Mods.Clear();
            ModsNodeViewModel baseNode = new ModsNodeViewModel(null, null, "");
            string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
            foreach (string modPath in modsPath)
                baseNode.Nodes.Add(new ModsNodeViewModel(baseNode, getModName(PathMod.GetModDetails(modPath)), modPath));
            Mods.Add(baseNode);
        }

        private static string getModName(ModHeader mod)
        {
            if (!mod.IsValid())
                return null;
            return mod.GetMenuName();
        }
    }
}
