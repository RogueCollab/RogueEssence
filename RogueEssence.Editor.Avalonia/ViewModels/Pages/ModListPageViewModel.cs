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
    
    public ObservableCollection<ModsEntryViewModel> FilteredItems { get; } = new();
    
    public async void btnSwitch_Click()
    {
        bool isCurrentMod = await _modManager.CheckIsCurrentMod(SelectedItem);

        if (!isCurrentMod)
        {
            await _modManager.AskSwitchTo(SelectedItem);
        }
    }

  
    ModManagerViewModel _modManager;

    public ModListPageViewModel(EditorContext context, ModManagerViewModel modManager, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) :
        base(context, node, onPageOpen)
    {
        _modManager = modManager;
    }
    private void UpdateVisibleItems(string filter)
    {
        FilteredItems.Clear();
        var strategy = new BeginningTitleFilterStrategy();
        foreach (var item in _modManager.ModsList.Where(e => strategy.Matches(e.Name, filter) || strategy.Matches(e.Namespace, filter)))
            FilteredItems.Add(item);
    }

    public override void OnPageLoad()
    {
        base.OnPageLoad();
        this.WhenAnyValue(x => x.SearchFilter).Subscribe(UpdateVisibleItems);
    }

    
    public async void btnAdd_Click()
    {
        ModNameWindowViewModel vm = new ModNameWindowViewModel();

        bool result = await _context.DialogService.ShowDialogAsync<ModNameWindowViewModel, bool>(vm, "Add Mod and Namespace");
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
        foreach (ModsEntryViewModel child in _modManager.ModsList)
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

        ModsEntryViewModel newEntry =
            new ModsEntryViewModel(newName, newNamespace, Path.Combine(PathMod.MODS_FOLDER, newName));
        string fullPath = PathMod.FromApp(newEntry.Path);
        //add all asset folders
        Directory.CreateDirectory(fullPath);
        //create the mod xml
        ModHeader newHeader = new ModHeader(fullPath, vm.Name.Trim(), "", "",
            Text.Sanitize(vm.Namespace).ToLower(), Guid.NewGuid(), new Version(),
            new Version(), PathMod.ModType.Mod, new RelatedMod[0]);
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
        _modManager.ModsList.Add(newEntry);
        UpdateVisibleItems(SearchFilter);
        await AddChildItemUnderParent(newEntry);
    }
    
    public async void btnDelete_Click()
    {
        //prohibit the deletion of the current node or the base node
        if (SelectedItem == _modManager.ModsList[0])
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

        //and then delete mod
        _modManager.RemoveMod(SelectedItem);
        UpdateVisibleItems(SearchFilter);
        
    }
    
    public async Task AddChildItemUnderParent(ModsEntryViewModel entry)
    {
        if (entry == _modManager.ModsList[0])
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
                        string fullPath = PathMod.FromApp(page.Path);
                        ModHeader resultHeader = new ModHeader(PathMod.Quest.Path, page.Name.Trim(), page.Author.Trim(), page.Description.Trim(), Text.Sanitize(page.Namespace).ToLower(), Guid.Parse(page.UUID), Version.Parse(page.Vers), Version.Parse(page.GameVersion), (PathMod.ModType)page.ChosenModType, page.GetRelationshipArray());
                        PathMod.SaveModDetails(fullPath, resultHeader);
                    
                        _modManager.ReloadMods();
                        UpdateVisibleItems(SearchFilter);
                    };
                }));
            NodeHelper.ExpandParents(Node, true);
        }

    }

}