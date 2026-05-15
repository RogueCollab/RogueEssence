using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels;

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

public class ModListPageViewModel : EditorPageViewModel
{
    private string _searchFilter = string.Empty;

    public string SearchFilter
    {
        get => _searchFilter;
        set => this.RaiseAndSetIfChanged(ref _searchFilter, value);
    }


    private ModsEntryViewModel? _selectedItem;

    public ModsEntryViewModel? SelectedItem
    {
        get => _selectedItem;
        set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
    }


    public ObservableCollection<ModsEntryViewModel> Items { get; } = new();
    public ObservableCollection<ModsEntryViewModel> FilteredItems { get; } = new();

    public ObservableCollection<ModsEntryViewModel> EditMenuItems { get; } = new();


    private string currentMod;

    public string CurrentMod
    {
        get => currentMod;
        set => this.SetIfChanged(ref currentMod, value);
    }

    public async void btnSwitch_Click()
    {
        //give a pop up warning that the game will be reloaded and wait for confirmation
        MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_context.DialogService,
            $"The game will be reloaded to use content from {SelectedItem.Namespace}.\nClick OK to proceed.", "Are you sure?",
            MessageBoxWindowView.MessageBoxButtons.OkCancel);
        if (result == MessageBoxWindowView.MessageBoxResult.Cancel)
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
            if (!String.IsNullOrEmpty(SelectedItem.Path))
                GameManager.Instance.SetQuest(PathMod.GetModDetails(PathMod.FromApp(SelectedItem.Path)),
                    new ModHeader[0] { }, new List<int>() { -1 });
            else
                GameManager.Instance.SetQuest(ModHeader.Invalid, new ModHeader[0] { }, new List<int>() { });

            DiagManager.Instance.PrintModSettings();
            DiagManager.Instance.SaveModSettings();
        }
    }

    public ModListPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) :
        base(context, node, onPageOpen)
    {
    }
    private void UpdateVisibleItems(string filter)
    {
        FilteredItems.Clear();
        var strategy = new BeginningTitleFilterStrategy();
        foreach (var item in Items.Where(e => strategy.Matches(e.Name, filter) || strategy.Matches(e.Namespace, filter)))
            FilteredItems.Add(item);
    }

    public override void OnPageLoad()
    {
        base.OnPageLoad();
        currentMod = null;
        this.WhenAnyValue(x => x.SearchFilter).Subscribe(UpdateVisibleItems);

        // Items = new ObservableCollection<ModsEntryViewModel>();
        reloadMods();
       
    }

    
    public async void btnAdd_Click()
    {
        ModConfigWindowView window = new ModConfigWindowView();
        ModHeader header = new ModHeader("", "", "", "", "", Guid.NewGuid(), new Version(), new Version(),
            PathMod.ModType.Mod, new RelatedMod[0] { });
        ModConfigViewModel2 vm = new ModConfigViewModel2(_context.DialogService, header);
        window.DataContext = vm;

        DevForm form = (DevForm)DiagManager.Instance.DevEditor;
        bool result = await window.ShowDialog<bool>(form);
        if (!result)
            return;

        string newName = Text.Sanitize(vm.Name);
        string newNamespace = Text.Sanitize(vm.Namespace).ToLower();

        //sanitize name and check for name conflicts
        if (String.IsNullOrWhiteSpace(newName))
            return;
        if (String.IsNullOrWhiteSpace(newNamespace))
            return;

        //check for children name conflicts
        foreach (ModsEntryViewModel child in Items)
        {
            if (String.Equals(child.Name, newName, StringComparison.OrdinalIgnoreCase))
            {
                //already exists, pop up message
                await MessageBoxWindowView.Show(_context.DialogService, newName + " already exists!",
                    "Add Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }

            if (String.Equals(child.Namespace, newNamespace, StringComparison.OrdinalIgnoreCase))
            {
                //already exists, pop up message
                await MessageBoxWindowView.Show(_context.DialogService, newName + " (Namespace) already exists!",
                    "Add Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
                return;
            }
        }

        ModsEntryViewModel newNode =
            new ModsEntryViewModel(newName, newNamespace, Path.Combine(PathMod.MODS_FOLDER, newName));
        string fullPath = PathMod.FromApp(newNode.Path);
        //add all asset folders
        Directory.CreateDirectory(fullPath);
        //create the mod xml
        ModHeader newHeader = new ModHeader(fullPath, vm.Name.Trim(), vm.Author.Trim(), vm.Description.Trim(),
            Text.Sanitize(vm.Namespace).ToLower(), Guid.Parse(vm.UUID), Version.Parse(vm.Version),
            Version.Parse(vm.GameVersion), (PathMod.ModType)vm.ChosenModType, vm.GetRelationshipArray());
        PathMod.SaveModDetails(fullPath, newHeader);

        //add Strings
        Directory.CreateDirectory(Path.Join(fullPath, "Strings"));
        //Content
        GraphicsManager.InitContentFolders(fullPath);
        //Data
        DataManager.InitDataDirs(fullPath);
        //Script
        LuaEngine.InitScriptFolders(fullPath, vm.Namespace);

        //add node
        Items.Add(newNode);
        UpdateVisibleItems(SearchFilter);
    }
    
    public async void btnDelete_Click()
    {
        //prohibit the deletion of the current node or the base node
        if (SelectedItem == Items[0])
        {
            await MessageBoxWindowView.Show(_context.DialogService, "Cannot delete the root mod!", "Delete Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            return;
        }
        //ask for confirmation
        MessageBoxWindowView.MessageBoxResult result = await MessageBoxWindowView.Show(_context.DialogService, "Are you sure you want to delete the mod in directory:\n" + SelectedItem.Path, "Are you sure?",
            MessageBoxWindowView.MessageBoxButtons.YesNo);
        if (result == MessageBoxWindowView.MessageBoxResult.No)
            return;

        string fullPath = PathMod.FromApp(SelectedItem.Path);
        //delete folder
        Directory.Delete(fullPath, true);

        //and then delete node
        Items.Remove(SelectedItem);
    }
    
    public async void btnEdit_Click()
    {
        ModConfigWindowView window = new ModConfigWindowView();
        ModHeader header = PathMod.Quest;
        ModConfigViewModel2 vm = new ModConfigViewModel2(_context.DialogService, header);
        window.DataContext = vm;

        DevForm form = (DevForm)DiagManager.Instance.DevEditor;
        bool result = await window.ShowDialog<bool>(form);

        if (result)
        {
            //save the mod data
            string fullPath = PathMod.FromApp(PathMod.Quest.Path);
            ModHeader resultHeader = new ModHeader(PathMod.Quest.Path, vm.Name.Trim(), vm.Author.Trim(), vm.Description.Trim(), Text.Sanitize(vm.Namespace).ToLower(), Guid.Parse(vm.UUID), Version.Parse(vm.Version), Version.Parse(vm.GameVersion), (PathMod.ModType)vm.ChosenModType, vm.GetRelationshipArray());
            PathMod.SaveModDetails(fullPath, resultHeader);

            reloadMods();
            DevForm.ExecuteOrPend(doSwitch);
        }
    }

    public async Task AddChildItemUnderParent(ModsEntryViewModel entry)
    {
        if (entry == Items[0])
        {
            await MessageBoxWindowView.Show(_context.DialogService, "Cannot edit the root mod!", "Edit Failed", MessageBoxWindowView.MessageBoxButtons.Ok);
            
        }
        else
        {
            Node.AddNodeIfNotExists(_context.NodeFactory.CreatModItemNode<ModConfigPageViewModel>(entry.Path, entry.Display, Node.Icon,
                async (page) =>
                {

                    page.OnOKValidAction = () =>
                    {
                        string fullPath = PathMod.FromApp(PathMod.Quest.Path);
                        ModHeader resultHeader = new ModHeader(PathMod.Quest.Path, page.Name.Trim(), page.Author.Trim(), page.Description.Trim(), Text.Sanitize(page.Namespace).ToLower(), Guid.Parse(page.UUID), Version.Parse(page.Vers), Version.Parse(page.GameVersion), (PathMod.ModType)page.ChosenModType, page.GetRelationshipArray());
                        PathMod.SaveModDetails(fullPath, resultHeader);
                    
                        reloadMods();
                        // DevForm.ExecuteOrPend(doSwitch);
                    };
                }));
            NodeHelper.ExpandParents(Node, true);
        }

    }
    
    private void reloadMods()
    {
        Items.Clear();
        Items.Add(new ModsEntryViewModel("Origin", PathMod.BaseNamespace, ""));
        string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
        ModsEntryViewModel chosenModel = null;
        foreach (string modPath in modsPath)
        {
            ModHeader header = PathMod.GetModDetails(modPath);
            Items.Add(new ModsEntryViewModel(getModName(header), header.Namespace, Path.Combine(PathMod.MODS_FOLDER, Path.GetFileName(modPath))));
            if (PathMod.Quest.Path == header.Path)
                chosenModel = Items[Items.Count - 1];
        }
        SelectedItem = chosenModel;
        UpdateVisibleItems(SearchFilter);
    }

    private static string getModName(ModHeader mod)
    {
        if (!mod.IsValid())
            return null;
        return mod.GetMenuName();
    }

}