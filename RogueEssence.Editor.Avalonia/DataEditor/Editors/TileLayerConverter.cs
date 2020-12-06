using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System.Collections;
using Avalonia;
using System.Reactive.Subjects;

namespace RogueEssence.Dev
{
    public class TileLayerConverter : EditorConverter<TileLayer>
    {

        //else if (type == typeof(TileLayer))
        //{
        //    LoadLabelControl(control, name);

        //    TilePreview preview = new TilePreview();
        //    preview.Dock = DockStyle.Fill;
        //    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
        //    preview.SetChosenAnim((TileLayer)member);
        //    control.Controls.Add(preview);
        //    preview.TileClick += (object sender, EventArgs e) =>
        //    {
        //        ElementForm frmData = new ElementForm();
        //        frmData.Text = name + "/" + "Tile";

        //        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
        //        int box_down = 0;
        //        LoadLabelControl(frmData.ControlPanel, name);
        //        box_down += 16;
        //        //for enums, use a combobox
        //        TileBrowser browser = new TileBrowser();
        //        browser.Location = new Point(boxRect.Left, box_down);
        //        browser.Size = new Size(boxRect.Width, boxRect.Height);
        //        browser.SetBrush(preview.GetChosenAnim());
        //        frmData.ControlPanel.Controls.Add(browser);

        //        if (frmData.ShowDialog() == DialogResult.OK)
        //            preview.SetChosenAnim(browser.GetBrush());
        //    };
        //}
        //else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
        //{
        //    LoadLabelControl(control, name);

        //    TilePreview preview = new TilePreview();
        //    preview.Dock = DockStyle.Fill;
        //    preview.Size = new Size(GraphicsManager.TileSize, GraphicsManager.TileSize);
        //    preview.SetChosenAnim(((IList<TileLayer>)member).Count > 0 ? ((IList<TileLayer>)member)[0] : new TileLayer());
        //    control.Controls.Add(preview);

        //    CollectionBox lbxValue = new CollectionBox();
        //    lbxValue.Dock = DockStyle.Fill;
        //    lbxValue.Size = new Size(0, 175);
        //    lbxValue.LoadFromList(type, (IList)member);
        //    control.Controls.Add(lbxValue);

        //    lbxValue.SelectedIndexChanged += (object sender, EventArgs e) =>
        //    {
        //        if (lbxValue.SelectedIndex > -1)
        //            preview.SetChosenAnim(((IList<TileLayer>)lbxValue.Collection)[lbxValue.SelectedIndex]);
        //        else
        //            preview.SetChosenAnim(((IList<TileLayer>)lbxValue.Collection).Count > 0 ? ((IList<TileLayer>)lbxValue.Collection)[0] : new TileLayer());
        //    };


        //    //add lambda expression for editing a single element
        //    lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
        //    {
        //        ElementForm frmData = new ElementForm();
        //        frmData.Text = name + "/" + "Tile #" + index;

        //        Rectangle boxRect = new Rectangle(new Point(), new Size(654, 502 + LABEL_HEIGHT));
        //        int box_down = 0;
        //        LoadLabelControl(frmData.ControlPanel, name);
        //        box_down += 16;
        //        //for enums, use a combobox
        //        TileBrowser browser = new TileBrowser();
        //        browser.Location = new Point(boxRect.Left, box_down);
        //        browser.Size = new Size(boxRect.Width, boxRect.Height);
        //        browser.SetBrush(element != null ? (TileLayer)element : new TileLayer());

        //        frmData.OnOK += (object okSender, EventArgs okE) =>
        //        {
        //            element = browser.GetBrush();
        //            frmData.Close();
        //        };
        //        frmData.OnCancel += (object okSender, EventArgs okE) =>
        //        {
        //            frmData.Close();
        //        };

        //        frmData.ControlPanel.Controls.Add(browser);

        //        frmData.Show();
        //    };

        //}








        //else if (type == typeof(TileLayer))
        //{
        //    controlIndex++;
        //    TilePreview preview = (TilePreview)control.Children[controlIndex];
        //    member = preview.GetChosenAnim();
        //    controlIndex++;
        //}
        //else if (type.GetInterfaces().Contains(typeof(IList<TileLayer>)))
        //{
        //    controlIndex++;
        //    controlIndex++;
        //    CollectionBox lbxValue = (CollectionBox)control.Children[controlIndex];
        //    member = lbxValue.Collection;
        //    controlIndex++;
        //}
    }
}
