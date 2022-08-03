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

    public class EntryDataEditor : StringEditor
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override Type GetAttributeType() { return typeof(DataTypeAttribute); }

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, string member, Type[] subGroupStack)
        {
            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            ComboBox cbValue = new ComboBox();
            cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;


            List<string> items = new List<string>();
            if (dataAtt.IncludeInvalid)
                items.Add("**EMPTY**");

            int chosenIndex = 0;
            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];

            foreach (string key in nameIndex.Entries.Keys)
            {
                if (key == member)
                    chosenIndex = items.Count;
                items.Add(key + ": " + nameIndex.Entries[key].GetLocalString(true));
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
            control.Children.Add(cbValue);
        }


        public override string SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            ComboBox cbValue = (ComboBox)control.Children[controlIndex];
            int chosenIndex = cbValue.SelectedIndex;
            if (dataAtt.IncludeInvalid)
                chosenIndex--;

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];
            int curIndex = 0;
            foreach (string key in nameIndex.Entries.Keys)
            {
                if (curIndex == chosenIndex)
                    return key;
            }

            return "";
        }

        public override string GetString(string obj, Type type, object[] attributes)
        {
            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];
            if (nameIndex.Entries.ContainsKey(obj))
                return nameIndex.Entries[obj].Name.ToLocal();
            return obj;
        }
    }
}
