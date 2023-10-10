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
using System.Text.RegularExpressions;
using System.Diagnostics.Metrics;

namespace RogueEssence.Dev
{

    public class StringKeyEditor : Editor<StringKey>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, StringKey member, Type[] subGroupStack)
        {
            StringKeyAttribute dataAtt = ReflectionExt.FindAttribute<StringKeyAttribute>(attributes);

            ComboBox cbValue = new SearchComboBox();
            cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;

            List<string> items = new List<string>();

            int chosenIndex = 0;
            List<string> totalKeys = new List<string>();

            if (dataAtt != null && dataAtt.IncludeInvalid)
                items.Add("**EMPTY**");

            foreach (string key in Text.StringsEx.Keys)
                totalKeys.Add(key);
            totalKeys.Sort();
            foreach (string key in totalKeys)
            {
                if (key == member.Key)
                    chosenIndex = items.Count;
                items.Add(String.Format("{0}: {1}", key, Text.StringsEx[key]));
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
            control.Children.Add(cbValue);
        }


        public override StringKey SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            StringKeyAttribute dataAtt = ReflectionExt.FindAttribute<StringKeyAttribute>(attributes);

            ComboBox cbValue = (ComboBox)control.Children[controlIndex];


            List<string> items = new List<string>();
            if (dataAtt != null && dataAtt.IncludeInvalid)
                items.Add("");

            List<string> totalKeys = new List<string>();
            foreach (string key in Text.StringsEx.Keys)
                totalKeys.Add(key);
            totalKeys.Sort();
            items.AddRange(totalKeys);


            return new StringKey(items[cbValue.SelectedIndex]);
        }

        public override string GetString(StringKey obj, Type type, object[] attributes)
        {
            if (obj.IsValid())
                return String.Format("{0}: {1}", obj.Key, obj.ToLocal());
            return "**EMPTY**";
        }
    }
}
