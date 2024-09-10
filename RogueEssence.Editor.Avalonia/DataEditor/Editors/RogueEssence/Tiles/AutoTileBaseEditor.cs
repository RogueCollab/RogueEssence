using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public class AutoTileBaseEditor : Editor<AutoTileBase>
    {

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
                bool advancedEdit = false;
                DataEditForm frmData = new DataEditForm();
                frmData.Title = "Choose a Tilesheet";

                object[] elementAttr = new object[1];
                elementAttr[0] = new AnimAttribute(0, "Tile");
                DataEditor.LoadClassControls(frmData.ControlPanel, parent, parentType, name, typeof(string), elementAttr, "", true, new Type[0], advancedEdit);
                DataEditor.TrackTypeSize(frmData, typeof(string));

                frmData.SelectedOKEvent += async () =>
                {
                    object element = DataEditor.SaveClassControls(frmData.ControlPanel, name, typeof(string), elementAttr, true, new Type[0], advancedEdit);
                    string destSheet = (string)element;
                    //change all tiles of this object by first saving the object and then updating and then reloading?
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

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };
            control.Children.Add(btnAssign);
        }

    }
}
