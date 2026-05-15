using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using RogueEssence.Dev.Services;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev.ViewModels;

public class ModConfigPageViewModel : EditorPageViewModel
{
    public Action OnOKValidAction;
    
    private string _path;

    public ModConfigPageViewModel(EditorContext context, ModItemNode node,
        Action<EditorPageViewModel> onPageOpen = null) : base(context, node, onPageOpen)
    {
        _path = node.Path;
    }

    public void LoadDataFromPath()
    {
        // string fullPath = PathMod.FromApp(PathMod.Quest.Path);
        // ModHeader resultHeader = new ModHeader(PathMod.Quest.Path, vm.Name.Trim(), vm.Author.Trim(), vm.Description.Trim(), Text.Sanitize(vm.Namespace).ToLower(), Guid.Parse(vm.UUID), Version.Parse(vm.Version), Version.Parse(vm.GameVersion), (PathMod.ModType)vm.ChosenModType, vm.GetRelationshipArray());
        // PathMod.SaveModDetails(fullPath, resultHeader);


        ModHeader header = PathMod.GetModDetails(PathMod.FromApp(_path));
        Name = header.Name;
        Namespace = header.Namespace;
        Author = header.Author;
        Description = header.Description;
        UUID = header.UUID.ToString().ToUpper();
        Vers = header.Version.ToString();
        GameVersion = header.GameVersion.ToString();

        for (int ii = 0; ii < (int)PathMod.ModType.Count; ii++)
            ModTypes.Add(((PathMod.ModType)ii).ToLocal());
        ChosenModType = (int)header.ModType;

        Relationships =
            new CollectionBoxViewModel(_context.DialogService, new StringConv(typeof(RelatedMod), new object[0]));
        Relationships.OnEditItem += Relationships_EditItem;
        Relationships.LoadFromList(header.Relationships);
    }


    public override void OnPageLoad()
    {
        base.OnPageLoad();
        LoadDataFromPath();
    }


    // public ModConfigWindowViewModel(IDialogService dialogService, ModHeader header)
    // {
    //     Name = header.Name;
    //     Namespace = header.Namespace;
    //     Author = header.Author;
    //     Description = header.Description;
    //     UUID = header.UUID.ToString().ToUpper();
    //     Version = header.Version.ToString();
    //     GameVersion = header.GameVersion.ToString();
    //
    //     ModTypes = new ObservableCollection<string>();
    //     for (int ii = 0; ii < (int)PathMod.ModType.Count; ii++)
    //         ModTypes.Add(((PathMod.ModType)ii).ToLocal());
    //     ChosenModType = (int)header.ModType;
    //
    //     DevForm form = (DevForm)DiagManager.Instance.DevEditor;
    //     Relationships = new CollectionBoxViewModel(dialogService, new StringConv(typeof(RelatedMod), new object[0]));
    //     Relationships.OnEditItem += Relationships_EditItem;
    //     Relationships.LoadFromList(header.Relationships);
    // }

    private string name;

    public string Name
    {
        get => name;
        set => this.SetIfChanged(ref name, value);
    }

    private string author;

    public string Author
    {
        get => author;
        set => this.SetIfChanged(ref author, value);
    }

    private string description;

    public string Description
    {
        get => description;
        set => this.SetIfChanged(ref description, value);
    }

    private string editNamespace;

    public string Namespace
    {
        get => editNamespace;
        set => this.SetIfChanged(ref editNamespace, value);
    }

    private string uuid;

    public string UUID
    {
        get => uuid;
        set => this.SetIfChanged(ref uuid, value);
    }


    private string vers;

    public string Vers
    {
        get => vers;
        set => this.SetIfChanged(ref vers, value);
    }


    private string gameVersion;

    public string GameVersion
    {
        get => gameVersion;
        set => this.SetIfChanged(ref gameVersion, value);
    }

    public ObservableCollection<string> ModTypes { get; } = new();

    private int chosenModType;

    public int ChosenModType
    {
        get => chosenModType;
        set => this.SetIfChanged(ref chosenModType, value);
    }

    public CollectionBoxViewModel Relationships { get; set; }

    public void Relationships_EditItem(int index, object element, bool advancedEdit,
        CollectionBoxViewModel.EditElementOp op)
    {
        EditorPageViewModel pageViewModel = this;

        string elementName = "Relationship[" + index + "]";
        string title =
            DataEditor.GetWindowTitle("Relationship", elementName, element, typeof(RelatedMod), new object[0]);

        NodeBase node =
            _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName,
                pageViewModel.Node.Icon, pageViewModel.Node);
        pageViewModel.Node.AddNodeIfNotExists(node);
        NodeHelper.ExpandParents(node, true);

        ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);

        newEditor.SetPageTitle(elementName, pageViewModel.Node.Icon);
        newEditor.OnLoadAction = panel =>
        {
            newEditor.SetPageTitle(elementName, pageViewModel.Node.Icon);
            DataEditor.LoadClassControls(panel, "Relationship", null, elementName, typeof(RelatedMod),
                new object[0], element, true, new Type[0], advancedEdit);
        };

        newEditor.OnOKAction = async panel =>
        {
            element = DataEditor.SaveClassControls(panel, elementName, typeof(RelatedMod), new object[0],
                true, new Type[0], advancedEdit);

            bool itemExists = false;

            List<RelatedMod> relations = Relationships.GetList<List<RelatedMod>>();
            for (int ii = 0; ii < relations.Count; ii++)
            {
                if (ii != index)
                {
                    if (relations[ii].UUID == ((RelatedMod)element).UUID)
                        itemExists = true;
                }
            }

            if (String.IsNullOrEmpty(((RelatedMod)element).Namespace))
            {
                await MessageBoxWindowView.Show(_context.DialogService, "Related mod needs a namespace!",
                    "Namespace needed.",
                    MessageBoxWindowView.MessageBoxButtons.Ok);
                return false;
            }

            if (itemExists)
            {
                await MessageBoxWindowView.Show(_context.DialogService, "Cannot add duplicate entries.",
                    "Entry with UUID already exists.",
                    MessageBoxWindowView.MessageBoxButtons.Ok);
                return false;
            }
            else
            {
                op(index, element);
                return true;
            }
        };
        
        _context.TabEvents.AddChildPage(pageViewModel, newEditor);
    }


    public RelatedMod[] GetRelationshipArray()
    {
        List<RelatedMod> relationships = Relationships.GetList<List<RelatedMod>>();
        return relationships.ToArray();
    }

    public void btnRegenUUID_Click()
    {
        UUID = Guid.NewGuid().ToString().ToUpper();
    }


    public override void OnPageRemoved()
    {
        RemoveNodeFromTree();
        base.OnPageRemoved();
    }

    public void RemoveNodeFromTree()
    {
        Node.Parent?.SubNodes.Remove(Node);
    }
    
    public void Close()
    {
        _context.TabEvents.RemoveTab(this);
        RemoveNodeFromTree();
    }
    
    public async Task<bool> Validate()
    {
        var result = false;
        try
        {
            if (String.IsNullOrWhiteSpace(Text.Sanitize(Name)))
                throw new InvalidOperationException("Invalid Name");
            if (String.IsNullOrWhiteSpace(Text.Sanitize(Namespace).ToLower()))
                throw new InvalidOperationException("Invalid Namespace");

            Guid uuid = Guid.Parse(UUID);
            if (uuid == Guid.Empty)
                throw new InvalidOperationException("Invalid UUID");

            Version.Parse(Vers);
            Version.Parse(GameVersion);

            if (ChosenModType < 0 || ChosenModType >= (int)PathMod.ModType.Count)
                throw new InvalidOperationException("Invalid ModType");

            result = true;
        }
        catch (Exception ex)
        {
            DiagManager.Instance.LogError(ex, false);
            await MessageBoxWindowView.Show(_context.DialogService, ex.Message, "Invalid Input",
                MessageBoxWindowView.MessageBoxButtons.Ok);
        }

        return result;
    }
}