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

        private string editNamespace;
        public string Namespace
        {
            get => editNamespace;
            set => this.SetIfChanged(ref editNamespace, value);
        }

        public string Path;

        public ObservableCollection<ModsNodeViewModel> Nodes { get; }

        public ModsNodeViewModel(string name, string newNamespace, string fullPath)
        {
            this.name = name;
            this.editNamespace = newNamespace;
            this.Path = fullPath;
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
            if (chosenMod.Path == PathMod.Quest.Path)
                return;

            //give a pop up warning that the game will be reloaded and wait for confirmation
            MessageBox.MessageBoxResult result = await MessageBox.Show((Window)DiagManager.Instance.DevEditor, "The game will be reloaded to use content from the new path.\nClick OK to proceed.", "Are you sure?",
                MessageBox.MessageBoxButtons.OkCancel);
            if (result == MessageBox.MessageBoxResult.Cancel)
                return;

            DevForm.ExecuteOrPend(doSwitch);
        }

        private void doSwitch()
        {
            //modify and reload
            lock (GameBase.lockObj)
            {
                LuaEngine.Instance.BreakScripts();
                MenuManager.Instance.ClearMenus();
                if (!String.IsNullOrEmpty(chosenMod.Path))
                    GameManager.Instance.SetQuest(PathMod.GetModDetails(PathMod.FromExe(chosenMod.Path)), new ModHeader[0] { }, new List<int>() { -1 });
                else
                    GameManager.Instance.SetQuest(ModHeader.Invalid, new ModHeader[0] { }, new List<int>() { });
            }
        }

        public async void btnAdd_Click()
        {
            ModConfigWindow window = new ModConfigWindow();
            ModHeader header = new ModHeader("", "", "", Guid.NewGuid(), new Version(), PathMod.ModType.Mod, new RelatedMod[0] { });
            ModConfigViewModel vm = new ModConfigViewModel(header);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            bool result = await window.ShowDialog<bool>(form);
            if (!result)
                return;

            string newName = Text.Sanitize(vm.Name);
            string newNamespace = Text.Sanitize(vm.Namespace).ToLower();

            //sanitize name and check for name conflicts
            if (newName == "")
                return;
            if (newNamespace == "")
                return;

            //check for children name conflicts
            foreach (ModsNodeViewModel child in Mods)
            {
                if (String.Equals(child.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    //already exists, pop up message
                    await MessageBox.Show((Window)DiagManager.Instance.DevEditor, newName + " already exists!", "Add Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
                if (String.Equals(child.Namespace, newNamespace, StringComparison.OrdinalIgnoreCase))
                {
                    //already exists, pop up message
                    await MessageBox.Show((Window)DiagManager.Instance.DevEditor, newName + " (Namespace) already exists!", "Add Failed", MessageBox.MessageBoxButtons.Ok);
                    return;
                }
            }

            ModsNodeViewModel newNode = new ModsNodeViewModel(newName, newNamespace, Path.Combine(PathMod.MODS_FOLDER, newName));
            string fullPath = PathMod.FromExe(newNode.Path);
            //add all asset folders
            Directory.CreateDirectory(fullPath);
            //create the mod xml
            ModHeader newHeader = new ModHeader(fullPath, vm.Name.Trim(), Text.Sanitize(vm.Namespace).ToLower(), Guid.Parse(vm.UUID), Version.Parse(vm.Version), (PathMod.ModType)vm.ChosenModType, vm.GetRelationshipArray());
            PathMod.SaveModDetails(fullPath, newHeader);

            //add Strings
            Directory.CreateDirectory(Path.Join(fullPath, "Strings"));
            //Content
            GraphicsManager.InitContentFolders(fullPath);
            //Data
            DataManager.InitDataDirs(fullPath);
            //Script
            LuaEngine.InitScriptFolders(fullPath);

            //add node
            Mods.Add(newNode);
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
            MessageBox.MessageBoxResult result = await MessageBox.Show((Window)DiagManager.Instance.DevEditor, "Are you sure you want to delete the mod in directory:\n" + chosenMod.Path, "Are you sure?",
                MessageBox.MessageBoxButtons.YesNo);
            if (result == MessageBox.MessageBoxResult.No)
                return;

            //delete folder
            Directory.Delete(chosenMod.Path, true);

            //and then delete node
            Mods.Remove(chosenMod);
        }

        public async void btnEdit_Click()
        {
            ModConfigWindow window = new ModConfigWindow();
            ModHeader header = PathMod.Quest;
            ModConfigViewModel vm = new ModConfigViewModel(header);
            window.DataContext = vm;

            DevForm form = (DevForm)DiagManager.Instance.DevEditor;
            bool result = await window.ShowDialog<bool>(form);

            if (result)
            {
                //save the mod data
                string fullPath = PathMod.FromExe(PathMod.Quest.Path);
                ModHeader resultHeader = new ModHeader(PathMod.Quest.Path, vm.Name.Trim(), Text.Sanitize(vm.Namespace).ToLower(), Guid.Parse(vm.UUID), Version.Parse(vm.Version), (PathMod.ModType)vm.ChosenModType, vm.GetRelationshipArray());
                PathMod.SaveModDetails(fullPath, resultHeader);
                PathMod.Quest = resultHeader;
                reloadMods();
            }
        }

        private void reloadMods()
        {
            Mods.Clear();
            Mods.Add(new ModsNodeViewModel(null, PathMod.BaseNamespace, ""));
            string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
            foreach (string modPath in modsPath)
            {
                ModHeader header = PathMod.GetModDetails(modPath);
                Mods.Add(new ModsNodeViewModel(getModName(header), header.Namespace, Path.Combine(PathMod.MODS_FOLDER, Path.GetFileName(modPath))));
            }
        }

        private static string getModName(ModHeader mod)
        {
            if (!mod.IsValid())
                return null;
            return mod.GetMenuName();
        }
    }
}
