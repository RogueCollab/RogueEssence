using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.Utility;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public class AutoTileBaseEditor : Editor<AutoTileBase>
    {
        public AutoTileBaseEditor(EditorContext context) : base(context) { }

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, AutoTileBase obj, Type[] subGroupStack)
        {
            base.LoadWindowControls(control, parent, parentType, name, type, attributes, obj, subGroupStack);

            Button btnAssign = new Button();
            btnAssign.Margin = new Avalonia.Thickness(0, 4, 0, 0);
            btnAssign.Content = "Mass Assign Sheet";
            // TODO: Add btnAssign.PointerReleased event instead of click for advancedEdit
            // btnAssign.PointerReleased
            btnAssign.Click += (object sender, RoutedEventArgs e) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                bool advancedEdit = false;
                string title = "Choose a Tilesheet";

                object[] elementAttr = new object[1];
                elementAttr[0] = new AnimAttribute(0, "Tile");

                NodeBase node = _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(title, pageViewModel.Node.Icon);
                pageViewModel.Node.AddNodeIfNotExists(node);

                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(title, pageViewModel.Node.Icon);

                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, parent, parentType, name, typeof(string), elementAttr, "", true, new Type[0], advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    object element = DataEditor.SaveClassControls(stack, name, typeof(string), elementAttr, true, new Type[0], advancedEdit);
                    string destSheet = (string)element;

                    AutoTileBase preTiles = SaveWindowControls(control, name, type, attributes, subGroupStack);
                    foreach (List<TileLayer> layers in preTiles.IterateElements())
                    {
                        foreach (TileLayer layer in layers)
                        {
                            for (int ii = 0; ii < layer.Frames.Count; ii++)
                                layer.Frames[ii] = new TileFrame(layer.Frames[ii].TexLoc, destSheet);
                        }
                    }
                    LoadWindowControls(control, parent, parentType, name, type, attributes, preTiles, subGroupStack);

                    return true;
                };

                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };
            control.Children.Add(btnAssign);
        }

    }
}
