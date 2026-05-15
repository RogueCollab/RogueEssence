using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;


namespace RogueEssence.Dev.ViewModels;

public class TreeSearchViewModel : ViewModelBase
{
    public ObservableCollection<NodeBase> Nodes { get; }
    public ObservableCollection<NodeBase> SelectedNodes { get; }

    public TreeSearchViewModel()
    {
        // SelectedNodes = new ObservableCollection<NodeBase>();
        // Nodes = new ObservableCollection<NodeBase>
        // {
        //     new OpenEditorNode("Halcyon", "Icons.FloppyDiskBackFill")
        //     {
        //         SubNodes = new ObservableCollection<NodeBase>
        //         {
        //             new OpenEditorNode("Dev Control", "Icons.GameControllerFill", "DevControl"),
        //             new OpenEditorNode("Zone Editor", "Icons.StairsFill", "ZoneEditor"),
        //             new OpenEditorNode("Ground Editor", "Icons.MapTrifoldFill", "GroundEditor"),
        //             new OpenEditorNode("Testing", "Icons.BedFill", "RandomInfo"),
        //             new OpenEditorNode("Constants", "Icons.ListFill")
        //             {
        //                 SubNodes = new ObservableCollection<NodeBase>
        //                 {
        //                     new OpenEditorNode("Start Parameters", "Icons.ListFill"),
        //                     new OpenEditorNode("Universal Events", "Icons.ListFill"),
        //                     new OpenEditorNode("Strings", "Icons.TableFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new OpenEditorNode("English", "Icons.TableFill"),
        //                             new OpenEditorNode("Chinese", "Icons.TableFill")
        //                         }
        //                     },
        //                     new OpenEditorNode("Effects", "Icons.SparkleFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new OpenEditorNode("Heal FX", "Icons.SparkleFill"),
        //                             new OpenEditorNode("+Charge FX", "Icons.SparkleFill"),
        //                         }
        //                     },
        //                 }
        //             },
        //             new OpenEditorNode("Data", "Icons.FloppyDiskFill")
        //             {
        //                 SubNodes =
        //                 {
        //                     new DataRootNode("Monsters", "Monsters", "Monsters", "Icons.GhostFill")
        //                     {
        //                         SubNodes =
        //                         {
        //                             // TODO: Change later
        //                             new DataItemNode("eevee", "MonsterEditor", "eevee: Eevee", "Icons.GhostFill"),
        //                             new DataItemNode("seviper", "MonsterEditor", "seviper: Seviper", "Icons.GhostFill")
        //                         }
        //                     },
        //                     new ActionDataNode("Items", "Icons.JarLabelFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new NodeBase("ammo_cacnea_spike: Ammo Cacnea Spike", "Icons.JarLabelFill"),
        //                         }
        //                     },
        //                     new ActionDataNode("Zones", "Icons.StairsFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new NodeBase("ambush_forest: Ambush Forest", "Icons.StairsFill"),
        //                         }
        //                     },
        //                     new ActionDataNode("Statuses", "Icons.HeartFill")
        //                     {
        //                         SubNodes = new ObservableCollection<NodeBase>
        //                         {
        //                             new NodeBase("para: Paralyzed", "Icons.HeartFill"),
        //                         }
        //                     },
        //                 }
        //             },
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
        //     },
        // };
        // Nodes.First().IsExpanded = true;
    }
}