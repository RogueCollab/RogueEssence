using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;

namespace RogueEssence.Dev
{
    public class SpawnRangeListConverter : EditorConverter<ISpawnRangeList>
    {
        public override void LoadClassControls(ISpawnRangeList obj, StackPanel control)
        {
            //SpawnRangeListBox lbxValue = new SpawnRangeListBox();
            //lbxValue.Dock = DockStyle.Fill;
            //lbxValue.Size = new Size(0, 250);

            //Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnRangeList<>), obj.GetType(), 0);
            //lbxValue.StringConv = DataEditor.GetStringRep(elementType, new object[0] { });
            ////add lambda expression for editing a single element
            //lbxValue.OnEditItem = (int index, object element, SpawnRangeListBox.EditElementOp op) =>
            //{
            //    ElementForm frmData = new ElementForm();
            //    if (element == null)
            //        frmData.Text = "New " + elementType.Name;
            //    else
            //        frmData.Text = element.ToString();

            //    DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(SpawnRangeList) [" + index + "]", elementType, new object[0] { }, element, true);

            //    frmData.OnOK += (object okSender, EventArgs okE) =>
            //    {
            //        DataEditor.StaticSaveMemberControl(frmData.ControlPanel, "SpawnRangeList", elementType, new object[0] { }, ref element, true);

            //        op(index, element);
            //        frmData.Close();
            //    };
            //    frmData.OnCancel += (object okSender, EventArgs okE) =>
            //    {
            //        frmData.Close();
            //    };

            //    frmData.Show();
            //};

            //lbxValue.LoadFromList(obj.GetType(), obj);
            //control.Controls.Add(lbxValue);
        }


        public override void SaveClassControls(ISpawnRangeList obj, StackPanel control)
        {
            //SpawnRangeListBox lbxValue = (SpawnRangeListBox)control.Controls[0];

            //for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
            //    obj.Add(lbxValue.Collection.GetSpawn(ii), lbxValue.Collection.GetSpawnRange(ii), lbxValue.Collection.GetSpawnRate(ii));
        }
    }
}
