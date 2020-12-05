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
    public class SpawnListConverter : EditorConverter<ISpawnList>
    {
        public override void LoadClassControls(ISpawnList obj, StackPanel control)
        {
            //SpawnListBox lbxValue = new SpawnListBox();
            //lbxValue.Dock = DockStyle.Fill;
            //lbxValue.Size = new Size(0, 200);

            //Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnList<>), obj.GetType(), 0);
            //lbxValue.StringConv = DataEditor.GetStringRep(elementType, new object[0] { });
            ////add lambda expression for editing a single element
            //lbxValue.OnEditItem = (int index, object element, SpawnListBox.EditElementOp op) =>
            //{
            //    ElementForm frmData = new ElementForm();
            //    if (element == null)
            //        frmData.Text = "New " + elementType.Name;
            //    else
            //        frmData.Text = element.ToString();

            //    DataEditor.StaticLoadMemberControl(frmData.ControlPanel, "(SpawnList) [" + index + "]", elementType, new object[0] { }, element, true);

            //    frmData.OnOK += (object okSender, EventArgs okE) =>
            //    {
            //        DataEditor.StaticSaveMemberControl(frmData.ControlPanel, "SpawnList", elementType, new object[0] { }, ref element, true);

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
            //control.Children.Add(lbxValue);
        }


        public override void SaveClassControls(ISpawnList obj, StackPanel control)
        {
            //SpawnListBox lbxValue = (SpawnListBox)control.Controls[0];

            //for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
            //    obj.Add(lbxValue.Collection.GetSpawn(ii), lbxValue.Collection.GetSpawnRate(ii));
        }
    }
}
