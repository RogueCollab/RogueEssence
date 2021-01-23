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

        //    Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ISpawnRangeList<>), type, 0);
        //    //lbxValue.StringConv = DataEditor.GetStringRep(elementType, new object[0] { });
        //    //add lambda expression for editing a single element
        //    lbxValue.OnEditItem = (int index, object element, SpawnRangeListBox.EditElementOp op) =>
        //    {
        //        ElementForm frmData = new ElementForm();
        //        if (element == null)
        //            frmData.Text = name + "/" + "New " + elementType.Name;
        //        else
        //            frmData.Text = name + "/" + element.ToString();

        //        DataEditor.loadClassControls(frmData.ControlPanel, "(SpawnRangeList) [" + index + "]", elementType, ReflectionExt.GetPassableAttributes(2, attributes), element, true);

        //        frmData.OnOK += (object okSender, EventArgs okE) =>
        //        {
        //            DataEditor.saveClassControls(frmData.ControlPanel, name, elementType, ReflectionExt.GetPassableAttributes(2, attributes), ref element, true);

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
        //}
    }
}
