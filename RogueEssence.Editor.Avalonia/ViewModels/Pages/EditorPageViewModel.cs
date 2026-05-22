using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using RogueEssence.Dev.Services;


namespace RogueEssence.Dev.ViewModels;

public interface IPreCreatePage
{
    static virtual void OnPreCreate() { }
}


public interface IPageReloadable
{
    public void Reload();
}

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
    
    public void SetPageTitleFromNode(NodeBase node)
    {
        SetPageTitle(node.Title, node.Icon);
    }

    public void SetPageTitle(string title, string icon)
    {
        Title = title;
        Icon = icon;
    }
    
    public void SetTitle(string title)
    {
        Title = title;
    }
    
    public void SetIcon(string icon)
    {
        Icon = icon;
    }
}