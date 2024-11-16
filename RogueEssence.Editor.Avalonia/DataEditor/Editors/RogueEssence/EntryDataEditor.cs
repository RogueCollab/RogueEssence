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

            ComboBox cbValue = new SearchComboBox();


            List<string> items = new List<string>();
            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];
            List<string> orderedKeys = nameIndex.GetOrderedKeys(false);

            int chosenIndex = orderedKeys.IndexOf(member);
            if (dataAtt.IncludeInvalid)
            {
                items.Insert(0, "**EMPTY**");
                chosenIndex++;
            }

            foreach (string key in orderedKeys)
                items.Add(key + ": " + nameIndex.Get(key).GetLocalString(true));

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsSourceProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
            control.Children.Add(cbValue);
        }


        public override string SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            ComboBox cbValue = (ComboBox)control.Children[controlIndex];

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];
            List<string> orderedKeys = nameIndex.GetOrderedKeys(false);
            if (dataAtt.IncludeInvalid)
                orderedKeys.Insert(0, "");

            return orderedKeys[cbValue.SelectedIndex];
        }

        public override string GetString(string obj, Type type, object[] attributes)
        {
            DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[dataAtt.DataType];
            if (nameIndex.ContainsKey(obj))
                return nameIndex.Get(obj).Name.ToLocal();
            return obj;
        }
    }
}
