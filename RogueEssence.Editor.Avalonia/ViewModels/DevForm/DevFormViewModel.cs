using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;
using RogueEssence.Dev.Models;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Content;
using RogueEssence.Data;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;
using RogueEssence.Dungeon;
using RogueEssence.Menu;
using RogueEssence.Script;

namespace RogueEssence.Dev.ViewModels;

public class DevFormViewModel : ViewModelBase
{
    private HierarchicalTreeDataGridSource<NodeBase> _nodeSource;

    public HierarchicalTreeDataGridSource<NodeBase> NodeSource
    {
        get => _nodeSource;
        set => this.RaiseAndSetIfChanged(ref _nodeSource, value);
    }

    public DevTabGameViewModel Game { get; set; }
    public DevTabPlayerViewModel Player { get; set; }
    public DevTabTravelViewModel Travel { get; set; }
    public DevTabScriptViewModel Script { get; set; }

    public ModManagerViewModel ModsManager { get; set; }
    
 

    private bool _isTreeView;

    public bool IsTreeView
    {
        get => _isTreeView;
        set { this.RaiseAndSetIfChanged(ref _isTreeView, value); }
    }

    public void OpenTabSwitcher()
    {
        TabSwitcher = _context.PageFactory.GetRequiredService<TabSwitcherViewModel>();
    }

    public event Action? TabSwitcherClosed;

    public void CloseTabSwitcher()
    {
        TabSwitcher = null;
        TabSwitcherClosed?.Invoke();
    }

    public void OnModSwitcherOpened()
    {
        if (ModSwitcher == null)
        {
            // TODO: Doesn't look ideal
            ModSwitcher = _context.PageFactory.GetRequiredService<ModSwitcherViewModel>();
        }
    }


    public event Action? ModSwitcherClosed;

    public void OnModSwitcherClosed()
    {
        if (ModSwitcher != null)
        {
            ModSwitcher = null;
            ModSwitcherClosed?.Invoke();
        }
    }


    private string _filter = "";

    public string Filter
    {
        get { return _filter; }
        set { this.RaiseAndSetIfChanged(ref _filter, value); }
    }


    private void ApplyFilter(string filter)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var node in Nodes)
            {
                NodeHelper.FilterRecursive(node, filter, new BeginningTitleFilterStrategy());
            }

            RefreshTreeDataGrid();
        });
    }

    private void RefreshTreeDataGrid()
    {
        NodeSource = new HierarchicalTreeDataGridSource<NodeBase>(Nodes)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<NodeBase>(
                    new TemplateColumn<NodeBase>(
                        null,
                        "TreeDataGridNodeBaseTemplate",
                        null,
                        new GridLength(1, GridUnitType.Star)),
                    x => x.SubNodes.Where(n => n.IsVisible),
                    x => x.SubNodes.Count > 0,
                    x => x.IsExpanded)
            },
        };
    }


    private ObservableCollection<EditorPageViewModel> _pages;

    public ObservableCollection<EditorPageViewModel> Pages
    {
        get => _pages;
        set => this.RaiseAndSetIfChanged(ref _pages, value);
    }

    private EditorPageViewModel? _activePage;

    public EditorPageViewModel? ActivePage
    {
        get => _activePage;
        set => this.RaiseAndSetIfChanged(ref _activePage, value);
    }

    private EditorPageViewModel? _temporaryTab;


    public EditorPageViewModel? TemporaryTab
    {
        get => _temporaryTab;
        set => this.RaiseAndSetIfChanged(ref _temporaryTab, value);
    }

    private ObservableCollection<PageNode> _topLevelPages;

    public ObservableCollection<PageNode> TopLevelPages
    {
        get => _topLevelPages;
        set => this.RaiseAndSetIfChanged(ref _topLevelPages, value);
    }

    private Dictionary<EditorPageViewModel, PageNode> _pageToNodeMap;


    public void AddTopLevelPage(EditorPageViewModel page)
    {
        var navigatedPage = TryNavigateToExistingPage(page);
        if (navigatedPage != null)
        {
            if (navigatedPage is IPageReloadable reloadable)
                reloadable.Reload();
            return;
        }

        if (!page.AddNewTab)
        {
            TemporaryTab = page;
            return;
        }

        // Only load the data once it is confirmed that the tab doesn't already exist
        page.OnPageLoad();

        Pages.Add(page);
        var node = _context.NodeFactory.CreatePageNode(page, null);
        TopLevelPages.Add(node);
        _pageToNodeMap[page] = node;
        ActivePage = page;
    }


    public void AddChildPage(EditorPageViewModel parentPage, EditorPageViewModel childPage)
    {
        var navigatedPage = TryNavigateToExistingPage(childPage);
        if (navigatedPage != null) return;


        // Otherwise... add to the list of tabs
        if (_pageToNodeMap.TryGetValue(parentPage, out var parentNode))
        {
            var childNode = parentNode.AddChild(childPage);
            _pageToNodeMap[childPage] = childNode;

            // Try to insert the child page to the right of the parent
            var parentIndex = Pages.IndexOf(parentPage);
            if (parentIndex == -1)
            {
                Pages.Add(childPage);
            }
            else
            {
                Pages.Insert(parentIndex + 1, childPage);
            }
        }
        else
        {
            AddTopLevelPage(parentPage);
        }
    }

    // Returns whether the navigation was successful
    private EditorPageViewModel? TryNavigateToExistingPage(EditorPageViewModel page)
    {
        var existing = Pages.FirstOrDefault(p => p.Equals(page));
        if (existing != null)
        {
            TemporaryTab = null;
            ActivePage = existing;
            return existing;
        }

        return null;
    }

    public bool PageHasChildren(EditorPageViewModel page)
    {
        if (!_pageToNodeMap.TryGetValue(page, out var node))
            return false;

        return node.SubNodes.Count > 0;
    }

    private PageNode GetTopLevelNode(PageNode node)
    {
        var current = node;
        while (!current.IsTopLevel)
            current = current.Parent;
        return current;
    }

    public void RemoveTab(EditorPageViewModel page)
    {
        // Console.WriteLine($"Removing tab {page}" + "hmmm");
        if (!_pageToNodeMap.TryGetValue(page, out var node))
            return;

        int removeIdx = Pages.IndexOf(page);
        
        // Console.WriteLine($"Removing tab {page} at index {removeIdx}");

        ClosePageAndChildren(node);


        // TODO:
        // We want to prioritize setting the left tab to be the active tab since our editors open stuff to the right first
        // Maybe we want to set the active page to be the parent if it exists?
        // For after deleting Datatype entries, should it go back to the last previously visited tab?
        if (Pages.Count == 0)
        {
            ActivePage = null;
        }
        else
        {
            if (removeIdx > 0 && removeIdx - 1 < Pages.Count)
            {
                ActivePage = Pages[removeIdx - 1];
            }
            else if (removeIdx > 0)
            {
                ActivePage = Pages[Pages.Count - 1];
            }
            else
            {
                ActivePage = Pages[0];
            }
        }
    }

    public async Task SaveChildren(EditorPageViewModel page)
    {
        if (!_pageToNodeMap.TryGetValue(page, out var pageNode))
            return;

        foreach (var child in pageNode.SubNodes.Cast<PageNode>().ToList())
        {
            await SaveChildren(child.Page);
            if (child.Page.AttachedView is ISaveable saveable)
                await saveable.Save();
        }
    }

    private void ClosePageAndChildren(PageNode node)
    {
        var children = node.SubNodes.Cast<PageNode>().ToList();
        foreach (var child in children)
        {
            ClosePageAndChildren(child);
        }

        Pages.Remove(node.Page);


        if (node.IsTopLevel)
        {
            TopLevelPages.Remove(node);
        }
        else
        {
            node.Parent.RemoveChild(node);
        }

        _pageToNodeMap.Remove(node.Page);
    }

    private ObservableCollection<NodeBase> _nodes = new();

    public ObservableCollection<NodeBase> Nodes
    {
        get => _nodes;
        set => this.RaiseAndSetIfChanged(ref _nodes, value);
    }
    
    private EditorContext _context;

    public DevFormViewModel(EditorContext context, DevTabGameViewModel game, DevTabPlayerViewModel player,
        DevTabTravelViewModel travel, DevTabScriptViewModel script,
        ModManagerViewModel mods)
    {
        _context = context;
        Game = game;
        Player = player;
        Travel = travel;
        Script = script;
        ModsManager = mods;
        
        
        
        InitializeTabEvents();

        this.WhenAnyValue(x => x.ActivePage)
            .Where(activePage => activePage != null)
            .Subscribe(_ => TemporaryTab = null);

    
        
        Pages = new ObservableCollection<EditorPageViewModel>();
        TopLevelPages = new ObservableCollection<PageNode>();
        _pageToNodeMap = new Dictionary<EditorPageViewModel, PageNode>();

        this.WhenAnyValue(x => x.ActivePage)
            .Buffer(2, 1)
            .Subscribe(pair =>
            {
                if (pair.Count > 1 && pair[0] != null) pair[0].IsActive = false;
                if (pair.Count > 0 && pair[^1] != null) pair[^1].IsActive = true;
            });

        this.WhenAnyValue(x => x.Filter).Throttle(TimeSpan.FromMilliseconds(300)).Subscribe(ApplyFilter);

        NodeSource = new HierarchicalTreeDataGridSource<NodeBase>(Nodes)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<NodeBase>(
                    new TemplateColumn<NodeBase>(
                        null,
                        "TreeDataGridNodeBaseTemplate",
                        null,
                        new GridLength(1, GridUnitType.Star)),
                    x => x.SubNodes.Where(n => n.IsVisible),
                    x => x.SubNodes.Count > 0,
                    x => x.IsExpanded)
            },
        };
    }

    public void ClearFilter()
    {
        Filter = "";
    }

    public async void ShowPreferencesWindow()
    {
        await _context.DialogService.ShowDialogAsync<PreferencesWindowViewModel, bool>(
            PreferencesWindowViewModel.Instance, "Preferences");
    }

    public NodeBase Root { get; internal set; }
    public NodeBase MapEditorNode { get; internal set; }
    
    public NodeBase GroundEditorNode { get; internal set; }
    
    public void LoadDevTree()
    {
        NodeFactory _nodeFactory = _context.NodeFactory;
        Filter = "";

        ActivePage = null;
        TopLevelPages.Clear();
        Pages.Clear();
        _pageToNodeMap.Clear();

        foreach (var n in Nodes)
        {
            DetachEventsRecursive(n);
        }

        Nodes.Clear();

        var rootStr = ModsManager.CurrentModString;
        
        var root = _nodeFactory.CreateOpenEditorNode<EmptyPageViewModel>(rootStr, "Icons.ScrollFill");

        Root = root;
        ModsManager.WhenAnyValue(x => x.CurrentMod.Name)
            .Subscribe(str => Root.Title = str ?? "Origin");
        
        var devControlNode =
            _nodeFactory.CreateOpenEditorNode<DevControlPageViewModel>("Dev Control", "Icons.GameControllerFill");
        root.SubNodes.Add(devControlNode);
        
        var tab = _context.PageFactory.CreatePage(typeof(DevControlPageViewModel), devControlNode);
        tab.SetPageTitle("Dev Control", "Icons.GameControllerFill");
        AddTopLevelPage(tab);


        var mapEditorNode = _nodeFactory.CreateOpenEditorNode<MapEditPageViewModel>("Map Editor", "Icons.StairsFill");
        root.SubNodes.Add(mapEditorNode);
        
        MapEditorNode = mapEditorNode;

        var groundEditorNode =
            _nodeFactory.CreateOpenEditorNode<GroundEditPageViewModel>("Ground Editor", "Icons.IslandFill");

        root.SubNodes.Add(groundEditorNode);
        GroundEditorNode = groundEditorNode;

        CreateDataNode(root);
        CreateConstantsNode(root);
        
        CreateSpriteNode(root);

        CreateModNode(root);
        Nodes.Add(root);

        AttachEventsRecursive(root);
        root.IsExpanded = true;
    }
    
    private Action<ReflectedDataPageViewModel> CreateDataOnOpen<T>(Func<T> getter, Action<T> setter, NodeBase parent)
    {
        return vm =>
        {
            vm.SetRemoveNode(false);
            vm.SetRootPage(true);
            vm.OnLoadAction = stack => { DataEditor.LoadDataControls("", getter(), stack); };

            vm.OnOKAction = async stack =>
            {
                lock (GameBase.lockObj)
                {
                    object obj = getter();
                    DataEditor.SaveDataControls(ref obj, stack, new Type[0]);
                    setter((T)obj);
                }

                return true;
            };
        };
    }

    private Action<ReflectedDataPageViewModel> CreateFXOnOpen<T>(Func<T> getter, Action<T> setter, string fxKey,
        NodeBase parent)
        => CreateDataOnOpen(getter, fx =>
        {
            setter(fx);
            DataManager.SaveData(fx, DataManager.FX_PATH, fxKey, DataManager.DATA_EXT);
        }, parent);


    private void CreateConstantsNode(NodeBase parent)
    {
        var constantsNode =
            _context.NodeFactory.CreateReflectedDataNode<EmptyPageViewModel>("Constants", parent, "Icons.ListFill");
        var startParamsNode = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("Start Params",
            parent,
            "Icons.ListFill", CreateDataOnOpen(
                () => DataManager.Instance.Start,
                obj =>
                {
                    DataManager.Instance.Start = obj;
                    DataManager.Instance.SaveStartParams();
                },
                parent));

        var universalEventsNode = _context.NodeFactory.CreateUniversalNode<ReflectedDataPageViewModel>(
            "Universal Event", "Icons.ListFill",
            CreateDataOnOpen(
                () => (UniversalBaseEffect)DataManager.Instance.UniversalEvent,
                obj =>
                {
                    DataManager.Instance.UniversalEvent = obj;
                    DataManager.SaveData(obj, DataManager.DATA_PATH, "Universal", DataManager.DATA_EXT);
                },
                parent));

        
        var stringsNode = _context.NodeFactory.CreateOpenEditorNode<EmptyPageViewModel>("Strings", "Icons.TableFill");
        var menuTextNode = _context.NodeFactory.CreateOpenEditorNodeWithParams<StringEditPageViewModel>("Menu Text", [false], "Icons.TableFill");
        var gameplayTextNode = _context.NodeFactory.CreateOpenEditorNodeWithParams<StringEditPageViewModel>("Gameplay Text", [true], "Icons.TableFill");
        stringsNode.SubNodes.Add(menuTextNode);
        stringsNode.SubNodes.Add(gameplayTextNode);
        constantsNode.SubNodes.Add(startParamsNode);
        constantsNode.SubNodes.Add(universalEventsNode);
        constantsNode.SubNodes.Add(stringsNode);
        
        var effectsNode =
            _context.NodeFactory.CreateOpenEditorNode<EmptyPageViewModel>("Effects", "Icons.SparkleFill");
        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("Heal FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.HealFX, fx => DataManager.Instance.HealFX = fx, "Heal", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "Restore Charge FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.RestoreChargeFX, fx => DataManager.Instance.RestoreChargeFX = fx,
                "RestoreCharge", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "Lose Charge FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.LoseChargeFX, fx => DataManager.Instance.LoseChargeFX = fx,
                "LoseCharge", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "No Charge FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.NoChargeFX, fx => DataManager.Instance.NoChargeFX = fx,
                "NoCharge", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("Element FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.ElementFX, fx => DataManager.Instance.ElementFX = fx, "Element",
                parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "Intrinsic FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.IntrinsicFX, fx => DataManager.Instance.IntrinsicFX = fx,
                "Intrinsic", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "Send Home FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.SendHomeFX, fx => DataManager.Instance.SendHomeFX = fx,
                "SendHome", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "Item Lost FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.ItemLostFX, fx => DataManager.Instance.ItemLostFX = fx,
                "ItemLost", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("Warp FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.WarpFX, fx => DataManager.Instance.WarpFX = fx, "Warp", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(
            "Knockback FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.KnockbackFX, fx => DataManager.Instance.KnockbackFX = fx,
                "Knockback", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("Jump FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.JumpFX, fx => DataManager.Instance.JumpFX = fx, "Jump", parent)));

        effectsNode.SubNodes.Add(_context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>("Throw FX",
            effectsNode,
            "Icons.SparkleFill", CreateFXOnOpen(() => DataManager.Instance.ThrowFX, fx => DataManager.Instance.ThrowFX = fx, "Throw",
                parent)));

        constantsNode.SubNodes.Add(effectsNode);
        parent.SubNodes.Add(constantsNode);
    }

    private void CreateDataNode(NodeBase parent)
    {
        var dataNode =
            _context.NodeFactory.CreateOpenEditorNode<EmptyPageViewModel>("Data", "Icons.FloppyDiskFill");
        foreach (var type in Enum.GetValues<DataManager.DataType>())
        {
            if (type is DataManager.DataType.All or DataManager.DataType.None)
                continue;

            var entry = DataRegistry.Map[type];

            var dataItemRootNode = _context.NodeFactory.CreateDataRootNode<DataListPageViewModel>(
                type,
                type.ToString(),
                entry.Icon);
            dataNode.SubNodes.Add(dataItemRootNode);
        }

        parent.SubNodes.Add(dataNode);
    }

    private void CreateSpriteNode(NodeBase parent)
    {
        var spriteNode =
            _context.NodeFactory.CreateOpenEditorNode<EmptyPageViewModel>("Sprites", "Icons.PaintBrushFill");
        //
        spriteNode.SubNodes.Add(
            _context.NodeFactory.CreateOpenEditorNodeWithParams<SpeciesEditPageViewModel>("Char Sprites", [true],
                "Icons.PersonFill")
        );
        spriteNode.SubNodes.Add(
            _context.NodeFactory.CreateOpenEditorNodeWithParams<SpeciesEditPageViewModel>("Portraits", [false],
                "Icons.UserSquareFill")
        );
     
        foreach (var type in Enum.GetValues<GraphicsManager.AssetType>())
        {
            if (type == GraphicsManager.AssetType.None || type == GraphicsManager.AssetType.All ||
                type == GraphicsManager.AssetType.Count || type == GraphicsManager.AssetType.Autotile ||
                type == GraphicsManager.AssetType.Chara || type == GraphicsManager.AssetType.Portrait ||
                type == GraphicsManager.AssetType.Font)
                continue;


            if (type != GraphicsManager.AssetType.Tile)
            {

                spriteNode.SubNodes.Add(
                    _context.NodeFactory.CreateSpriteRootNode<SpritePageViewModel>(type, type.ToString(),
                        type.GetIcon()));
            }
            else
            {
                spriteNode.SubNodes.Add(
                    _context.NodeFactory.CreateSpriteTileRootNode<SpritePageViewModel>(type.ToString(),
                        type.GetIcon()));
                
            }
        }


        parent.SubNodes.Add(spriteNode);
        
    }
    
    private void CreateModNode(NodeBase parent)
    {
        var modRoot = _context.NodeFactory.CreateOpenEditorNode<ModListPageViewModel>("Mods", "Icons.ScrollFill");


        parent.SubNodes.Add(modRoot);
    }

    private void InitializeTabEvents()
    {
        TabEvents _tabEvents = _context.TabEvents;
        _tabEvents.AddChildTabEvent += (parent, child) =>
        {
            AddChildPage(parent, child);
            ActivePage = child;
        };

        _tabEvents.SaveChildrenEvent += SaveChildren;

        _tabEvents.AddTopLevelTabEvent += (tab) => { AddTopLevelPage(tab); };

        _tabEvents.RemoveTabEvent += (tab) => { RemoveTab(tab); };

        _tabEvents.NavigateToTabEvent += (tab) => { ActivePage = tab; };

        // TODO: Check if this works if the page is a children of a tab. What if it closes the children first before the parent...
        _tabEvents.CloseTabsForEntry += (key, dataType) =>
        {
            var topLevelPagesToClose = Pages
                .OfType<ReflectedDataPageViewModel>()
                .Where(p => p.Node is DataItemNode dataItemNode
                            && dataItemNode.ItemKey == key
                            && dataItemNode.Parent is DataRootNode rootNode
                            && rootNode.DataType == dataType)
                .Cast<EditorPageViewModel>()
                .Select(page =>
                {
                    if (!_pageToNodeMap.TryGetValue(page, out var node)) return null;
                    if (node is not PageNode pageNode) return null;
                    return GetTopLevelNode(pageNode).Page;
                })
                .Where(p => p != null)
                .Distinct()
                .ToList();

            foreach (var page in topLevelPagesToClose)
                RemoveTab(page);
        };
    }

    private void AttachEventsRecursive(NodeBase node)
    {
        node.SubNodesChanged += () => OnSubNodesChanged();

        foreach (var child in node.SubNodes)
            AttachEventsRecursive(child);
    }

    private void DetachEventsRecursive(NodeBase node)
    {
        node.SubNodesChanged -= OnSubNodesChanged;

        foreach (var child in node.SubNodes)
            DetachEventsRecursive(child);
    }

    private void OnSubNodesChanged()
    {
        RefreshTreeDataGrid();
    }

    private TabSwitcherViewModel? _tabSwitcher;

    public TabSwitcherViewModel? TabSwitcher
    {
        get => _tabSwitcher;
        set => this.RaiseAndSetIfChanged(ref _tabSwitcher, value);
    }


    private ModSwitcherViewModel? _modSwitcher;

    public ModSwitcherViewModel? ModSwitcher
    {
        get => _modSwitcher;
        set => this.RaiseAndSetIfChanged(ref _modSwitcher, value);
    }

    public void AddPageFromTreeNode(OpenEditorNode node)
    {
        if (node.EditorType.IsAssignableTo(typeof(IPreCreatePage)))
            node.EditorType.GetMethod("OnPreCreate")?.Invoke(null, null);
        
        var editor = _context.PageFactory.CreatePage(node.EditorType, node, node.OnPageLoad);
        
        if (editor != null)
        {
            editor.SetPageTitleFromNode(node);
            AddTopLevelPage(editor);
        }
    }

    public async Task<bool> TryCloseTabAsync(EditorPageViewModel page)
    {
        if (PageHasChildren(page))
        {
            // TODO: Add a can close method and check if any of the subtabs has any unsaved changes.
            var result = await MessageBoxWindowView.Show(_context.DialogService,
                "Are you sure you want to close all subtabs?  Your changes will not be saved.", "Confirm Close",
                MessageBoxWindowView.MessageBoxButtons.YesNo);

            if (result != MessageBoxWindowView.MessageBoxResult.Yes)
                return false;
        }

        RemoveTab(page);
        page.OnPageRemoved();
        return true;
    }


    public void OpenMapEditor()
    {
        var page = _context.PageFactory.CreatePage<MapEditPageViewModel>(MapEditorNode);
        page.SetPageTitle("Map Editor", "Icons.StairsFill");
        AddTopLevelPage(page);
    }
    public void OpenGroundEditor()
    {
        var page = _context.PageFactory.CreatePage<GroundEditPageViewModel>(GroundEditorNode);
        page.SetPageTitle("Ground Editor", "Icons.IslandFill");
        AddTopLevelPage(page);
    }
}