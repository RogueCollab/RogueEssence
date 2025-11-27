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
    public DevTabDataViewModel Data { get; set; }
    public DevTabTravelViewModel Travel { get; set; }
    public DevTabSpritesViewModel Sprites { get; set; }

    public DevTabScriptViewModel Script { get; set; }

    // public DevTabModsViewModel Mods { get; set; }
    public DevTabConstantsViewModel Constants { get; set; }


    private readonly NodeFactory _nodeFactory;

    public ObservableCollection<ModsNodeViewModel> Mods { get; }


    private ModsNodeViewModel _chosenMod;

    public ModsNodeViewModel ChosenMod
    {
        get => _chosenMod;
        set { this.RaiseAndSetIfChanged(ref _chosenMod, value); }
    }

    private string currentMod;

    public string CurrentMod
    {
        get => currentMod;
        set => this.SetIfChanged(ref currentMod, value);
    }

    public void UpdateMod()
    {
        CurrentMod = _getModName(PathMod.Quest);
    }

    private void _reloadMods()
    {
        Mods.Clear();

        string[] modsPath = Directory.GetDirectories(PathMod.MODS_PATH);
        ModsNodeViewModel chosenModel = new ModsNodeViewModel("Origins", PathMod.BaseNamespace, "");
        Mods.Add(chosenModel);
        foreach (string modPath in modsPath)
        {
            ModHeader header = PathMod.GetModDetails(modPath);
            Mods.Add(new ModsNodeViewModel(_getModName(header), header.Namespace,
                Path.Combine(PathMod.MODS_FOLDER, Path.GetFileName(modPath))));
            if (PathMod.Quest.Path == header.Path)
            {
                chosenModel = Mods[Mods.Count - 1];
            }
        }

        ChosenMod = chosenModel;
    }

    private void DoSwitch()
    {
        //modify and reload
        lock (GameBase.lockObj)
        {
            LuaEngine.Instance.BreakScripts();
            MenuManager.Instance.ClearMenus();
            if (!String.IsNullOrEmpty(_chosenMod.Path))
                GameManager.Instance.SetQuest(PathMod.GetModDetails(PathMod.FromApp(_chosenMod.Path)),
                    new ModHeader[0] { }, new List<int>() { -1 });
            else
                GameManager.Instance.SetQuest(ModHeader.Invalid, new ModHeader[0] { }, new List<int>() { });

            DiagManager.Instance.PrintModSettings();
            DiagManager.Instance.SaveModSettings();
        }
    }

    private static string _getModName(ModHeader mod)
    {
        if (!mod.IsValid())
            return null;
        return mod.GetMenuName();
    }

    private bool _isTreeView;

    public bool IsTreeView
    {
        get => _isTreeView;
        set { this.RaiseAndSetIfChanged(ref _isTreeView, value); }
    }

    public void OpenTabSwitcher()
    {
        TabSwitcher = _pageFactory.GetRequiredService<TabSwitcherViewModel>();
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
            ModSwitcher = _pageFactory.GetRequiredService<ModSwitcherViewModel>();
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

    public ReactiveCommand<Unit, Unit> OpenPreferencesWindow { get; }

    public ReactiveCommand<Unit, Unit> ClearFilterCommand { get; }

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
        Console.WriteLine($"Adding top level page {page}");
        var navigated = TryNavigateToExistingPage(page);
        if (navigated) return;

        if (!page.AddNewTab)
        {
            TemporaryTab = page;
            return;
        }


        Pages.Add(page);
        var node = _nodeFactory.CreatePageNode(page, null);
        TopLevelPages.Add(node);
        _pageToNodeMap[page] = node;
        ActivePage = page;
    }


    public void AddChildPage(EditorPageViewModel parentPage, EditorPageViewModel childPage)
    {
        var navigated = TryNavigateToExistingPage(childPage);
        if (navigated) return;


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
    private bool TryNavigateToExistingPage(EditorPageViewModel page)
    {
        var existing = Pages.FirstOrDefault(p => p.Equals(page));
        if (existing != null)
        {
            TemporaryTab = null;
            ActivePage = existing;
            return true;
        }

        return false;
    }
    
    public bool PageHasChildren(EditorPageViewModel page)
    {
        if (!_pageToNodeMap.TryGetValue(page, out var node))
            return false;

        return node.SubNodes.Count > 0;
    }

    public void RemoveTab(EditorPageViewModel page)
    {
        if (!_pageToNodeMap.TryGetValue(page, out var node))
            return;

        int removeIdx = Pages.IndexOf(page);

        ClosePageAndChildren(node);


        // We want to prioritize setting the left tab to be the active tab since our editors open stuff to the right first
        // Maybe we want to set the active page to be the parent if it exists?
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


    private readonly PageFactory _pageFactory;
    private readonly TabEvents _tabEvents;
    private readonly IDialogService _dialogService;


    private ObservableCollection<NodeBase> _nodes = new();

    public ObservableCollection<NodeBase> Nodes
    {
        get => _nodes;
        set => this.RaiseAndSetIfChanged(ref _nodes, value);
    }
    // public DevFormViewModel() : this(new PageFactory(new DesignServiceProvider()),
    //     new NodeFactory(new DesignServiceProvider()), new DialogService(),
    //     new TabEvents(new PageFactory(new DesignServiceProvider())), new DevTabGameViewModel(), new DevTabPlayerViewModel())
    // {
    // }

    public DevFormViewModel(PageFactory pageFactory, NodeFactory nodeFactory, IDialogService dialogService,
        TabEvents tabEvents, DevTabGameViewModel game, DevTabPlayerViewModel player, DevTabDataViewModel data,
        DevTabTravelViewModel travel, DevTabSpritesViewModel sprites, DevTabScriptViewModel script,
        DevTabModsViewModel mods,
        DevTabConstantsViewModel constants)
    {
        Mods = new ObservableCollection<ModsNodeViewModel>();
        // NOTE: These should all be private readonly
        Game = game;
        Player = player;
        Data = data;
        Travel = travel;
        Sprites = sprites;
        Script = script;
        // Mods = mods;
        Constants = constants;
        _pageFactory = pageFactory;
        _tabEvents = tabEvents;
        _nodeFactory = nodeFactory;
        _dialogService = dialogService;

        InitializeTabEvents();

        this.WhenAnyValue(x => x.ActivePage)
            .Where(activePage => activePage != null)
            .Subscribe(_ => TemporaryTab = null);

        Pages = new ObservableCollection<EditorPageViewModel>();
        TopLevelPages = new ObservableCollection<PageNode>();
        _pageToNodeMap = new Dictionary<EditorPageViewModel, PageNode>();

        // TODO: move this own view
        ClearFilterCommand = ReactiveCommand.Create(() => { Filter = string.Empty; });

        OpenPreferencesWindow = ReactiveCommand.CreateFromTask(async () =>
        {
            await _dialogService.ShowDialogAsync<PreferencesWindowViewModel, bool>(
                PreferencesWindowViewModel.Instance, "Preferences");
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

    public void LoadDevTree()
    {
        Filter = "";
        _reloadMods();

        ActivePage = null;
        TopLevelPages.Clear();
        Pages.Clear();
        _pageToNodeMap.Clear();

        var tab = _pageFactory.CreatePage(typeof(DevControlViewModel));
        tab.Icon = "Icons.GameControllerFill";
        AddTopLevelPage(tab);


        foreach (var n in Nodes)
        {
            DetachEventsRecursive(n);
        }

        Nodes.Clear();

        var rootStr = ChosenMod.Name;

        // TODO: change to Mod Edit Page View Model
        var root = _nodeFactory.CreateOpenEditorNode<DevEditPageViewModel>(rootStr, "Icons.ScrollFill");


        // root.SubNodes.Add(
            // _nodeFactory.CreateOpenEditorNode("Dev Control",  typeof(DevControlViewModel), "Icons.GameControllerFill"));
        root.SubNodes.Add(_nodeFactory.CreateOpenEditorNode<ZoneEditorPageViewModel>("Zone Editor", "Icons.StairsFill"));
        // root.SubNodes.Add(
            // _nodeFactory.CreateOpenEditorNode("Ground Editor", typeof(GroundEditorPageViewModel), "Icons.MapTrifoldFill"));
        // root.SubNodes.Add(_nodeFactory.CreateOpenEditorNode("Testing", "Icons.BedFill", "RandomInfo"));
        // root.SubNodes.Add(_nodeFactory.CreateOpenEditorNode("Tab Test", "Icons.AirplaneFill", "SpritePage"));
        
        // var particlesRoot = _nodeFactory.CreateSpriteRootNode("particles", "", "Particles", "Icons.PaintBrushFill");
        // particlesRoot.SubNodes.Add(_nodeFactory.CreateDataItemNode("Acid_Blue", "SpriteEditor", "Acid_Blue",
        //     "Icons.PaintBrushFill"));
        // particlesRoot.SubNodes.Add(_nodeFactory.CreateDataItemNode("Acid_Red", "SpriteEditor", "Acid_Red",
        //     "Icons.PaintBrushFill"));
        //
        // halcyonNode.SubNodes.Add(particlesRoot);


        // CreateConstantsNode(root);
        // CreateDataNode(root);
        //
        CreateSpriteNode(root);

        // CreateModNode(root);
        Nodes.Add(root);

        AttachEventsRecursive(root);
        root.IsExpanded = true;
    }


    private void CreateConstantsNode(NodeBase parent)
    {
        // var constantsNode = _nodeFactory.CreateOpenEditorNode("Constants", "Icons.ListFill", "");
        //
        // var startParamsNode = _nodeFactory.CreateOpenEditorNode("Start Params", "Icons.ListFill", "");
        // var universalEventsNode = _nodeFactory.CreateOpenEditorNode("Universal Events", "Icons.ListFill", "");
        //
        // var menuTextNode = _nodeFactory.CreateOpenEditorNode("Menu Text", "Icons.TableFill", "");
        // var gameplayTextNode = _nodeFactory.CreateOpenEditorNode("Gameplay Text", "Icons.TableFill", "");
        //
        //
        // constantsNode.SubNodes.Add(startParamsNode);
        // constantsNode.SubNodes.Add(universalEventsNode);
        // constantsNode.SubNodes.Add(menuTextNode);
        // constantsNode.SubNodes.Add(gameplayTextNode);
        //
        // var effectsNode = _nodeFactory.CreateOpenEditorNode("Effects", "Icons.ListFill", "");
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Heal FX",
        //     () => DataManager.Instance.HealFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.HealFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Heal", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Restore Charge FX",
        //     () => DataManager.Instance.RestoreChargeFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.RestoreChargeFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "RestoreCharge", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Lose Charge FX",
        //     () => DataManager.Instance.LoseChargeFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.LoseChargeFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "LoseCharge", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<EmoteFX>(
        //     "No Charge FX",
        //     () => DataManager.Instance.NoChargeFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.NoChargeFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "NoCharge", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Element FX",
        //     () => DataManager.Instance.ElementFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.ElementFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Element", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Intrinsic FX",
        //     () => DataManager.Instance.IntrinsicFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.IntrinsicFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Intrinsic", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Send Home FX",
        //     () => DataManager.Instance.SendHomeFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.SendHomeFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "SendHome", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Item Lost FX",
        //     () => DataManager.Instance.ItemLostFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.ItemLostFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "ItemLost", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Warp FX",
        //     () => DataManager.Instance.WarpFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.WarpFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Warp", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Knockback FX",
        //     () => DataManager.Instance.KnockbackFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.KnockbackFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Knockback", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Jump FX",
        //     () => DataManager.Instance.JumpFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.JumpFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Jump", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // effectsNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNodeFX<BattleFX>(
        //     "Throw FX",
        //     () => DataManager.Instance.ThrowFX,
        //     (fx) =>
        //     {
        //         DataManager.Instance.ThrowFX = fx;
        //         DataManager.SaveData(fx, DataManager.FX_PATH, "Throw", DataManager.DATA_EXT);
        //     },
        //     "Icons.SparkleFill"
        // ));
        //
        // constantsNode.SubNodes.Add(effectsNode);
        // effectsNode.SubNodes.Add(_nodeFactory.Create<BattleFX>("Damage FX", ...params));
        // effectsNode.SubNodes.Add(_nodeFactory.Create<BattleFX>("Heal FX", ...params));


        // var effectsNode = _nodeFactory.CreateOpenEditorNode("Effec", "Icons.ListFill", "");


        // parent.SubNodes.Add(constantsNode);
    }

    private void CreateDataNode(NodeBase parent)
    {
        // var dataNode = _nodeFactory.CreateOpenEditorNode("Data", "Icons.FloppyDiskFill", "");
        // foreach (var type in Enum.GetValues<DataManager.DataType>())
        // {
        //     if (type == DataManager.DataType.All || type == DataManager.DataType.None)
        //         continue;
        //
        //
        //     var dataItemRootNode = _nodeFactory.CreateDataRootNode(
        //         type,
        //         "TODO",
        //         type.ToString(),
        //         type.GetIcon());
        //     dataNode.SubNodes.Add(dataItemRootNode);
        //     var entries = DataManager.Instance.DataIndices[type].GetLocalStringArray(true);
        //
        //     foreach (string key in entries.Keys)
        //     {
        //         var itemNode = _nodeFactory.CreateDataItemNode(
        //             key,
        //             "DevEditEditor",
        //             $"{key}: {entries[key]}",
        //             type.GetIcon());
        //
        //         dataItemRootNode.SubNodes.Add(itemNode);
        //     }
        // }
        //
        // parent.SubNodes.Add(dataNode);
    }

    private void CreateSpriteNode(NodeBase parent)
    {
        // var spritesViewModel = _pageFactory.GetRequiredService<DevTabSpritesViewModel>();
        var spriteNode = _nodeFactory.CreateOpenEditorNode<DevEditPageViewModel>("Sprites", "Icons.PaintBrushFill");
        //
        spriteNode.SubNodes.Add(
            _nodeFactory.CreateOpenEditorNodeWithParams<SpeciesEditPageViewModel>("Char Sprites", [true], "Icons.PersonFill")
        );
        spriteNode.SubNodes.Add(
            _nodeFactory.CreateOpenEditorNodeWithParams<SpeciesEditPageViewModel>("Portraits", [false], "Icons.PersonFill")
        );
        // );
        //
        // foreach (var type in Enum.GetValues<GraphicsManager.AssetType>())
        // {
        //     if (type == GraphicsManager.AssetType.None || type == GraphicsManager.AssetType.All ||
        //         type == GraphicsManager.AssetType.Count || type == GraphicsManager.AssetType.Autotile ||
        //         type == GraphicsManager.AssetType.Chara || type == GraphicsManager.AssetType.Portrait ||
        //         type == GraphicsManager.AssetType.Font)
        //         continue;
        //
        //     if (type == GraphicsManager.AssetType.Tile)
        //     {
        //         var tileRootNode = _nodeFactory.CreateSpriteTileRootNode(
        //             "",
        //             type.ToString(),
        //             type.GetIcon());
        //         lock (GameBase.lockObj)
        //         {
        //             foreach (string name in GraphicsManager.TileIndex.Nodes.Keys.OrderBy(n => n))
        //             {
        //                 var itemNode = _nodeFactory.CreateDataItemNode(
        //                     name,
        //                     "",
        //                     name,
        //                     type.GetIcon());
        //                 tileRootNode.SubNodes.Add(itemNode);
        //             }
        //         }
        //
        //         spriteNode.SubNodes.Add(tileRootNode);
        //         continue;
        //     }
        //
        //     string assetPattern = GraphicsManager.GetPattern(type);
        //     string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern),
        //         String.Format(Path.GetFileName(assetPattern), "*"));
        //
        //     var spriteRootNode = _nodeFactory.CreateSpriteRootNode(
        //         type,
        //         "TODO",
        //         type.ToString(),
        //         type.GetIcon());
        //     // dataNode.SubNodes.Add(dataItemRootNode);
        //     spriteNode.SubNodes.Add(spriteRootNode);
        //
        //
        //     lock (GameBase.lockObj)
        //     {
        //         for (int ii = 0; ii < dirs.Length; ii++)
        //         {
        //             string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
        //             // anims.Add(filename);
        //             var itemNode = _nodeFactory.CreateDataItemNode(
        //                 filename,
        //                 "",
        //                 filename,
        //                 type.GetIcon());
        //
        //             spriteRootNode.SubNodes.Add(itemNode);
        //         }
        //     }
        // }


        parent.SubNodes.Add(spriteNode);

        // lock (GameBase.lockObj)
        // {
        // anims.Clear();
        // Anims.Clear();
        // string assetPattern = GraphicsManager.GetPattern(assetType);
        // string[] dirs = PathMod.GetModFiles(Path.GetDirectoryName(assetPattern), String.Format(Path.GetFileName(assetPattern), "*"));
        // for (int ii = 0; ii < dirs.Length; ii++)
        // {
        //     string filename = Path.GetFileNameWithoutExtension(dirs[ii]);
        //     anims.Add(filename);
        // }
        // Anims.SetItems(anims);
        // }


        // var dataNode = _nodeFactory.CreateSpriteRootNode("aaaa", "Icons.FloppyDiskFill", "Sprite");

        // foreach (var type in Enum.GetValues<DataManager.DataType>())
        // {
        //     if (type == DataManager.DataType.All || type == DataManager.DataType.None)
        //         continue;
        //
        //
        //     var dataItemRootNode = _nodeFactory.CreateDataRootNode(
        //         type,
        //         "TODO",
        //         type.ToString(),
        //         type.GetIcon());
        //     dataNode.SubNodes.Add(dataItemRootNode);
        //     var entries = DataManager.Instance.DataIndices[type].GetLocalStringArray(true);
        //
        //     foreach (string key in entries.Keys)
        //     {
        //         var itemNode = _nodeFactory.CreateDataItemNode(
        //             key,
        //             "DevEditEditor",
        //             $"{key}: {entries[key]}",
        //             type.GetIcon());
        //
        //         dataItemRootNode.SubNodes.Add(itemNode);
        //     }
        // }
        //
        // parent.SubNodes.Add(dataNode);
    }

    private void HandleSprites()
    {
    }

    private void CreateModNode(NodeBase parent)
    {
        // var modsViewModel = _pageFactory.GetRequiredService<DevTabModsViewModel>();
        // var modRoot = _nodeFactory.CreateModRootNode("", "Mods", "Icons.ScrollFill");
        //
        // foreach (ModsNodeViewModel mod in modsViewModel.Mods)
        // {
        //     var name = mod.Namespace == "origin" ? "Origins" : mod.Name;
        //     var itemNode = _nodeFactory.CreateDataItemNode(
        //         mod.Namespace,
        //         "",
        //         $"{mod.Namespace}: {name}",
        //         "Icons.ScrollFill");
        //
        //     modRoot.SubNodes.Add(itemNode);
        // }
        //
        // parent.SubNodes.Add(modRoot);
    }

    private void InitializeTabEvents()
    {
        _tabEvents.AddChildTabEvent += (parent, child) =>
        {
            AddChildPage(parent, child);
            ActivePage = child;
        };

        _tabEvents.AddTopLevelTabEvent += (tab) => { AddTopLevelPage(tab); };

        _tabEvents.RemoveTabEvent += (tab) => { RemoveTab(tab); };

        _tabEvents.NavigateToTabEvent += (tab) => { ActivePage = tab; };
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
        var editor = _pageFactory.CreatePage(node.EditorType, node);

        Console.WriteLine("EDitpr" + editor);
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
            var result = await MessageBoxWindowView.Show(_dialogService,
                "Are you sure you want to close all subtabs?  Your changes will not be saved.", "Confirm Close",
                MessageBoxWindowView.MessageBoxButtons.YesNo);

            if (result != MessageBoxWindowView.MessageBoxResult.Yes)
                return false;
        }

        RemoveTab(page);
        return true;
    }


    // public TreeSearchViewModel TreeSearch { get; } = new TreeSearchViewModel();
}