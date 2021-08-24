using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using Avalonia.Controls;
using RogueEssence.Dev.Views;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev
{
    public class AutoTileEditor : Editor<AutoTile>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, AutoTile member)
        {
            LoadLabelControl(control, name);

            TileBox cbxValue = new TileBox();
            TileBoxViewModel mv = new TileBoxViewModel();
            cbxValue.DataContext = mv;

            //add lambda expression for editing a single element
            mv.OnEditItem += (AutoTile element, TileBoxViewModel.EditElementOp op) =>
            {
                TileEditForm frmData = new TileEditForm();
                TileEditViewModel tmv = new TileEditViewModel();
                frmData.DataContext = tmv;
                tmv.Name = name + "/" + type.Name;

                //load as if eyedropping
                tmv.TileBrowser.TileSize = GraphicsManager.TileSize;
                tmv.LoadTile(element);

                tmv.SelectedOKEvent += () =>
                {
                    element = tmv.GetTile();
                    op(element);
                    frmData.Close();
                };
                tmv.SelectedCancelEvent += () =>
                {
                    frmData.Close();
                };

                control.GetOwningForm().RegisterChild(frmData);
                frmData.Show();
            };
            mv.LoadFromSource(member);
            control.Children.Add(cbxValue);
        }

        public override AutoTile SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            int controlIndex = 0;
            controlIndex++;
            TileBox lbxValue = (TileBox)control.Children[controlIndex];
            TileBoxViewModel mv = (TileBoxViewModel)lbxValue.DataContext;
            return mv.Tile;
        }
    }
}
