using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using RogueEssence.Dev.Services;


namespace RogueEssence.Dev.ViewModels;

public abstract class EditorPageViewModel<TNode> : EditorPageViewModel
    where TNode : NodeBase
{
    
    // The node that opened this page
    public new TNode Node => (TNode)base.Node;

    protected EditorPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
    }
    protected override bool IsSamePage(EditorPageViewModel other)
    {
        var otherTyped = (EditorPageViewModel<TNode>)other;
        return Node.Equals(otherTyped.Node);
    }

    protected override int GetHashCodeCore() => Node.GetHashCode();
}


public class EditorPageViewModel : ViewModelBase, IEquatable<EditorPageViewModel>
{

    public Control? AttachedView { get; set; }
    
    public virtual string DefaultTitle => "DEFAULT TEXT";
    
    public event EventHandler? PageRemoved;
    
    public virtual void OnPageRemoved()
    {
        PageRemoved?.Invoke(this, EventArgs.Empty);
    }
    
    // Whether to add a new tab when this page is added
    public virtual bool AddNewTab => true;
    
    private Action<EditorPageViewModel>? _onPageLoad;
    
    // Only load data when there is no duplicate pages using the IsSamePage method
    public virtual void OnPageLoad()
    {
        _onPageLoad?.Invoke(this);
    }
    
        
    private string _title = "";
    public string Title
    {
        get => string.IsNullOrEmpty(_title) ? DefaultTitle : _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    
    public bool Equals(EditorPageViewModel? other)
    {
        if (other is null) return false;
        if (GetType() != other.GetType()) return false;
        return IsSamePage(other);
    }

    public override bool Equals(object? obj) => obj is EditorPageViewModel other && Equals(other);
    public override int GetHashCode() => GetHashCodeCore();
    
    protected virtual bool IsSamePage(EditorPageViewModel other) => true;
    protected virtual int GetHashCodeCore() => RuntimeHelpers.GetHashCode(this);
    //
    // private string _data = "";
    //
    //
    // public string Data
    // {
    //     get { return _data; }
    //     set
    //     {
    //         this.RaiseAndSetIfChanged(ref _data, value);
    //     }
    // }
    //
 

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }
    
    private string? _icon = "";
    public string Icon
    {
        get { return _icon; }
        set
        {
            this.RaiseAndSetIfChanged(ref _icon, value);
        }
    }
    
    
       private bool _modified = false;
        public bool Modified
        {
            get { return _modified; }
            set
            {
                this.RaiseAndSetIfChanged(ref _modified, value);
            }
        }
        
    public NodeBase Node { get; }
    
    protected readonly EditorContext _context;

    protected EditorPageViewModel(EditorContext context, NodeBase node, Action<EditorPageViewModel> onPageLoad = null)
    {
        Node = node;
        _context = context;
        _onPageLoad = onPageLoad;
    }
    
    // public (NodeBase node, ReflectedDataPageViewModel? editor) AddNewNodeAndTab(string title, string icon = "Icons.PaintBrushFill")
    // {
    //     NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(title, icon);
    //     Node.SubNodes.Add(node);
    //     ReflectedDataPageViewModel editor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
    //     if (editor != null)
    //     {
    //         editor.SetPageTitle(title, icon);
    //         _context.TabEvents.AddChildPage(this, editor);
    //     }
    //
    //     return (node, editor);
    // }
    //
    

    public void SetPageTitleFromNode(NodeBase node)
    {
        SetPageTitle(node.Title, node.Icon);
    }

    public void SetPageTitle(string title, string icon)
    {
        _title = title;
        _icon = icon;
    }
    
    public void SetTitle(string title)
    {
        _title = title;
    }
    
    public void SetIcon(string icon)
    {
        _icon = icon;
    }
    
    // public RepositoryNode Node
    // {
    //     get => _node;
    //     set => SetProperty(ref _node, value);
    // }

    // public LauncherPage()
    // {
    // _node = new RepositoryNode() { Id = Guid.NewGuid().ToString() };
    // _data = Welcome.Instance;

    // New welcome page will clear the search filter before.
    // Welcome.Instance.ClearSearchFilter();
    // }


    // public class LauncherPage : ObservableObject
    // {
    //     public RepositoryNode Node
    //     {
    //         get => _node;
    //         set => SetProperty(ref _node, value);
    //     }
    //
    //     public object Data
    //     {
    //         get => _data;
    //         set => SetProperty(ref _data, value);
    //     }
    //
    //     public Models.DirtyState DirtyState
    //     {
    //         get => _dirtyState;
    //         private set => SetProperty(ref _dirtyState, value);
    //     }
    //
    //     public Popup Popup
    //     {
    //         get => _popup;
    //         set => SetProperty(ref _popup, value);
    //     }
    //
    //     public AvaloniaList<Models.Notification> Notifications
    //     {
    //         get;
    //         set;
    //     } = new AvaloniaList<Models.Notification>();
    //
    //     public LauncherPage()
    //     {
    //         _node = new RepositoryNode() { Id = Guid.NewGuid().ToString() };
    //         _data = Welcome.Instance;
    //
    //         // New welcome page will clear the search filter before.
    //         Welcome.Instance.ClearSearchFilter();
    //     }
    //
    //     public LauncherPage(RepositoryNode node, Repository repo)
    //     {
    //         _node = node;
    //         _data = repo;
    //     }
    //
    //     public void ClearNotifications()
    //     {
    //         Notifications.Clear();
    //     }
    //
    //     public async Task CopyPathAsync()
    //     {
    //         if (_node.IsRepository)
    //             await App.CopyTextAsync(_node.Id);
    //     }
    //
    //     public void ChangeDirtyState(Models.DirtyState flag, bool remove)
    //     {
    //         var state = _dirtyState;
    //         if (remove)
    //         {
    //             if (state.HasFlag(flag))
    //                 state -= flag;
    //         }
    //         else
    //         {
    //             state |= flag;
    //         }
    //
    //         DirtyState = state;
    //     }
    //
    //     public bool CanCreatePopup()
    //     {
    //         return _popup is not { InProgress: true };
    //     }
    //
    //     public async Task ProcessPopupAsync()
    //     {
    //         if (_popup is { InProgress: false } dump)
    //         {
    //             if (!dump.Check())
    //                 return;
    //
    //             dump.InProgress = true;
    //
    //             try
    //             {
    //                 var finished = await dump.Sure();
    //                 if (finished)
    //                 {
    //                     dump.Cleanup();
    //                     Popup = null;
    //                 }
    //             }
    //             catch (Exception e)
    //             {
    //                 App.LogException(e);
    //             }
    //
    //             dump.InProgress = false;
    //         }
    //     }
    //
    //     public void CancelPopup()
    //     {
    //         if (_popup == null || _popup.InProgress)
    //             return;
    //
    //         _popup?.Cleanup();
    //         Popup = null;
    //     }
    //
    //     private RepositoryNode _node = null;

    //     private Models.DirtyState _dirtyState = Models.DirtyState.None;
    //     private Popup _popup = null;
    // }

}