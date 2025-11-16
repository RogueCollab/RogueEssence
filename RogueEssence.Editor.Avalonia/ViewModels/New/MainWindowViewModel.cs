using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using RogueEssence.Dev.Models;
using RogueEssence.Dev.Services;
using ReactiveUI;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public DevTabGameViewModel Game { get; set; }
    public DevTabPlayerViewModel Player { get; set; }
    public DevTabDataViewModel Data { get; set; }
    public DevTabTravelViewModel Travel { get; set; }
    public DevTabSpritesViewModel Sprites { get; set; }
    public DevTabScriptViewModel Script { get; set; }
    public DevTabModsViewModel Mods { get; set; }
    public DevTabConstantsViewModel Constants { get; set; }


    
    private readonly NodeFactory _nodeFactory;


    private bool _isTreeView;

    public bool IsTreeView
    {
        get => _isTreeView;
        set { this.RaiseAndSetIfChanged(ref _isTreeView, value); }
    }


    private Models.ModHeader _currentMod = new  Models.ModHeader("Halcyon", "halcyon");

    public Models.ModHeader CurrentMod
    {
        get => _currentMod;
        set { this.RaiseAndSetIfChanged(ref _currentMod, value); }
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
        foreach (var node in Nodes)
        {
            NodeHelper.FilterRecursive(node, filter, new BeginningTitleFilterStrategy());
        }
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
        // Console.WriteLine($"Adding top level page {page}");
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
        if (!string.IsNullOrEmpty(page.UniqueId))
        {
            var existing = Pages.FirstOrDefault(p => p.UniqueId == page.UniqueId);
            if (existing != null)
            {
                TemporaryTab = null;
                ActivePage = existing;
                return true;
            }
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
    public ObservableCollection<NodeBase> Nodes { get; set; }

    public MainWindowViewModel() : this(new PageFactory(new DesignServiceProvider()),
        new NodeFactory(new DesignServiceProvider()), new DialogService(),
        new TabEvents(new PageFactory(new DesignServiceProvider())))
    {
    }

    public MainWindowViewModel(PageFactory pageFactory, NodeFactory nodeFactory, IDialogService dialogService,
        TabEvents tabEvents)
    {
        Game = new DevTabGameViewModel();
        Player = new DevTabPlayerViewModel();
        Data = new DevTabDataViewModel();
        Travel = new DevTabTravelViewModel();
        Sprites = new DevTabSpritesViewModel();
        Script = new DevTabScriptViewModel();
        Mods = new DevTabModsViewModel();
        Constants = new DevTabConstantsViewModel();
        _pageFactory = pageFactory;
        _tabEvents = tabEvents;
        _nodeFactory = nodeFactory;
        _dialogService = dialogService;

        BuildNodes();
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
                PreferencesWindowViewModel.Instance, "Preferences", false);
        });

        var tab = _pageFactory.CreatePage("DevControl");
        tab.Icon = "Icons.GameControllerFill";
        AddTopLevelPage(tab);
        this.WhenAnyValue(x => x.Filter).Subscribe(ApplyFilter);
    }
    
    private void InitializeTabEvents()
    {
        _tabEvents.AddChildTabEvent += (parent, child) =>
        {
            AddChildPage(parent, child);
            ActivePage = child;
        };

        _tabEvents.AddTopLevelTabEvent += (tab) =>
        {
            AddTopLevelPage(tab);
        };
        
        _tabEvents.RemoveTabEvent += (tab) => { RemoveTab(tab); };
        
        _tabEvents.NavigateToTabEvent += (tab) =>
        {
            ActivePage = tab;
        };
    }

    private void BuildNodes()
    {
        var halcyonNode = _nodeFactory.CreateOpenEditorNode("Halcyon", "Icons.FloppyDiskBackFill", "ModInfoEditor");

        halcyonNode.SubNodes.Add(
            _nodeFactory.CreateOpenEditorNode("Dev Control", "Icons.GameControllerFill", "DevControl"));
        halcyonNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNode("Zone Editor", "Icons.StairsFill", "ZoneEditor"));
        halcyonNode.SubNodes.Add(
            _nodeFactory.CreateOpenEditorNode("Ground Editor", "Icons.MapTrifoldFill", "GroundEditor"));
        halcyonNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNode("Testing", "Icons.BedFill", "RandomInfo"));
        halcyonNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNode("Tab Test", "Icons.AirplaneFill", "SpritePage"));

        halcyonNode.SubNodes.Add(_nodeFactory.CreateOpenEditorNode("Constants", "Icons.ListFill"));

        var monstersRoot = _nodeFactory.CreateDataRootNode("Monsters", "Monsters", "Monsters", "Icons.GhostFill");


        //             new OpenEditorNode("Dev Control", "Icons.GameControllerFill", "DevControl"),
        //             new OpenEditorNode("Zone Editor", "Icons.StairsFill", "ZoneEditor"),
        //             new OpenEditorNode("Ground Editor", "Icons.MapTrifoldFill", "GroundEditor"),
        //             new OpenEditorNode("Testing", "Icons.BedFill", "RandomInfo"),


        monstersRoot.SubNodes.Add(_nodeFactory.CreateDataItemNode("eevee", "MonsterEditor", "eevee: Eevee",
            "Icons.GhostFill"));
        monstersRoot.SubNodes.Add(_nodeFactory.CreateDataItemNode("seviper", "MonsterEditor", "seviper: Seviper",
            "Icons.GhostFill"));

        var particlesRoot = _nodeFactory.CreateSpriteRootNode("particles", "", "Particles", "Icons.PaintBrushFill");
        particlesRoot.SubNodes.Add(_nodeFactory.CreateDataItemNode("Acid_Blue", "SpriteEditor", "Acid_Blue",
            "Icons.PaintBrushFill"));
        particlesRoot.SubNodes.Add(_nodeFactory.CreateDataItemNode("Acid_Red", "SpriteEditor", "Acid_Red",
            "Icons.PaintBrushFill"));

        halcyonNode.SubNodes.Add(particlesRoot);
        //             new NodeBase("Sprites", "Icons.PaintBrushFill")
        //             {
        //                 SubNodes = new ObservableCollection<NodeBase>
        //                 {
        //                     new NodeBase("Char Sprites", "Icons.GhostFill"),
        //                     new NodeBase("Portraits", "Icons.ImagesSquareFill"),
        //                     new ActionDataNode("Particles", "Icons.ShootingStarFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new NodeBase("Absorb", "Icons.ShootingStarFill"),
        //                             new NodeBase("Acid_Blue", "Icons.ShootingStarFill"),
        //                         }
        //                     },
        //                     new ActionDataNode("Beam", "Icons.HeadlightsFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new NodeBase("Beam_2", "Icons.HeadlightsFill"),
        //                             new NodeBase("Beam_Pink", "Icons.HeadlightsFill"),
        //                         }
        //                     },
        //                 }
        //             },
        //             new NodeBase("Mods", "Icons.SwordFill")
        //             {
        //                 SubNodes = new ObservableCollection<NodeBase>
        //                 {
        //                     new NodeBase("halcyon: Halcyon", "Icons.SwordFill"),
        //                     new NodeBase("zorea_mystery_dungeon: Zorea Mystery Dungeon", "Icons.SwordFill"),
        //                 }
        //             }
        //         }

        halcyonNode.SubNodes.Add(monstersRoot);
        Nodes = new ObservableCollection<NodeBase> { halcyonNode };

        halcyonNode.IsExpanded = true;
        monstersRoot.IsExpanded = true;
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
    

    // Navigate to a page from tab switcher
    public void NavigateToPage(PageNode node)
    {
        Console.WriteLine($"Navigating to page {node.Title}");
        Console.WriteLine("yay");
        ActivePage = node.Page;
    }
    
    public void AddPageFromPageNode(OpenEditorNode node)
    {
        var editor = _pageFactory.CreatePage(node.EditorKey);

        if (editor != null)
        {
            editor.SetTabInfo(node);
            AddTopLevelPage(editor);
        }
    }
    
    public async Task<bool> TryCloseTabAsync(EditorPageViewModel page)
    {
        if (PageHasChildren(page))
        {
            var result = await MessageBoxWindowView.Show(
                "Are you sure you want to close all subtabs?  Your changes will not be saved.",
                "Confirm Close",
                MessageBoxWindowView.MessageBoxButtons.YesNo,
                _dialogService
            );

            if (result != MessageBoxWindowView.MessageBoxResult.Yes)
                return false;
        }

        RemoveTab(page);
        return true;
    }
    

    // public TreeSearchViewModel TreeSearch { get; } = new TreeSearchViewModel();
}