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
    public class AliasDataEditor : IntEditor
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override Type GetAttributeType() { return typeof(AliasAttribute); }

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, Int32 member, Type[] subGroupStack)
        {
            AliasAttribute dataAtt = ReflectionExt.FindAttribute<AliasAttribute>(attributes);
            Dictionary<int, string> aliases = DevGraphicsManager.GetAlias(dataAtt.Name);

            if (aliases == null)
            {
                base.LoadWindowControls(control, parent, name, type, attributes, member, subGroupStack);
                return;
            }
            
            ComboBox cbValue = new ComboBox();
            cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
            int chosenIndex = -1;

            List<string> items = new List<string>();
            foreach (int key in aliases.Keys)
            {
                if (key == member)
                    chosenIndex = items.Count;
                items.Add(key.ToString() + ": " + aliases[key]);
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            if (chosenIndex > -1)
                cbValue.SelectedIndex = chosenIndex;
            control.Children.Add(cbValue);
        }


        public override Int32 SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            AliasAttribute dataAtt = ReflectionExt.FindAttribute<AliasAttribute>(attributes);
            Dictionary<int, string> aliases = DevGraphicsManager.GetAlias(dataAtt.Name);

            if (aliases == null)
                return base.SaveWindowControls(control, name, type, attributes, subGroupStack);

            int controlIndex = 0;
            ComboBox cbValue = (ComboBox)control.Children[controlIndex];

            int currentIndex = 0;
            foreach (int key in aliases.Keys)
            {
                if (currentIndex == cbValue.SelectedIndex)
                    return key;
                currentIndex++;
            }

            return 0;
        }

        public override string GetString(Int32 obj, Type type, object[] attributes)
        {
            AliasAttribute dataAtt = ReflectionExt.FindAttribute<AliasAttribute>(attributes);
            Dictionary<int, string> aliases = DevGraphicsManager.GetAlias(dataAtt.Name);

            if (aliases != null)
            {
                string result;
                if (aliases.TryGetValue(obj, out result))
                    return result;
            }

            return obj.ToString();
        }
    }
}
