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
using System.IO;

namespace RogueEssence.Dev
{
    public class EntryDataEditor : IntEditor
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override Type GetAttributeType() { return typeof(DataTypeAttribute); }

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, Int32 member, Type[] subGroupStack)
        {
            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            ComboBox cbValue = new ComboBox();
            cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
            int chosenIndex = member;
            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];

            List<string> items = new List<string>();
            if (dataAtt.IncludeInvalid)
            {
                items.Add("---");
                chosenIndex++;
            }

            for (int ii = 0; ii < nameIndex.Count; ii++)
                items.Add(ii.ToString() + ": " + nameIndex.Entries[ii].GetLocalString(true));

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
            control.Children.Add(cbValue);
        }


        public override Int32 SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            ComboBox cbValue = (ComboBox)control.Children[controlIndex];
            int returnValue = cbValue.SelectedIndex;
            if (dataAtt.IncludeInvalid)
                returnValue--;
            return returnValue;
        }

        public override string GetString(Int32 obj, Type type, object[] attributes)
        {
            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];
            if (obj >= 0 && obj < nameIndex.Count)
                return nameIndex.Entries[obj].Name.ToLocal();
            return "---";
        }
    }
}
