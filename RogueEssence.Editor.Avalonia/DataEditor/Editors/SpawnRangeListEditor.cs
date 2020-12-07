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

namespace RogueEssence.Dev
{
    public class SpawnRangeListEditor : Editor<ISpawnRangeList>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        //public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, ISpawnRangeList member)
        //{
        //    LoadLabelControl(control, name);
        //    SpawnRangeListBox lbxValue = new SpawnRangeListBox();

        //    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnRangeList<>), obj.GetType(), 0);
        //    lbxValue.StringConv = DataEditor.GetStringRep(elementType, new object[0] { });
        //    //add lambda expression for editing a single element
        //    lbxValue.OnEditItem = (int index, object element, SpawnRangeListBox.EditElementOp op) =>
        //    {
        //        ElementForm frmData = new ElementForm();
        //        if (element == null)
        //            frmData.Text = "New " + elementType.Name;
        //        else
        //            frmData.Text = element.ToString();

        //        DataEditor.loadClassControls(frmData.ControlPanel, "(SpawnRangeList) [" + index + "]", elementType, new object[0] { }, element, true);

        //        frmData.OnOK += (object okSender, EventArgs okE) =>
        //        {
        //            DataEditor.saveClassControls(frmData.ControlPanel, "SpawnRangeList", elementType, new object[0] { }, ref element, true);

        //            op(index, element);
        //            frmData.Close();
        //        };
        //        frmData.OnCancel += (object okSender, EventArgs okE) =>
        //        {
        //            frmData.Close();
        //        };

        //        frmData.Show();
        //    };

        //    lbxValue.LoadFromList(member);
        //    control.Children.Add(lbxValue);
        //}


        //public override ISpawnRangeList SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        //{
        //    int controlIndex = 0;
        //    controlIndex++;
        //    SpawnRangeListBox lbxValue = (SpawnRangeListBox)control.Children[controlIndex];
        //    return lbxValue.GetList(type);

        //    //Old
        //    SpawnRangeListBox lbxValue = (SpawnRangeListBox)control.Controls[0];
        //    for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
        //        obj.Add(lbxValue.Collection.GetSpawn(ii), lbxValue.Collection.GetSpawnRange(ii), lbxValue.Collection.GetSpawnRate(ii));
        //}
    }
}
