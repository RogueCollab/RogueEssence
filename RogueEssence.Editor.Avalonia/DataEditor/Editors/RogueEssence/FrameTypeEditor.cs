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
    public class FrameTypeEditor : IntEditor
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;

        public override Type GetAttributeType() { return typeof(FrameTypeAttribute); }

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Int32 member, Type[] subGroupStack)
        {
            FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);

            ComboBox cbValue = new ComboBox();
            cbValue.VirtualizationMode = ItemVirtualizationMode.Simple;
            int chosenIndex = 0;

            List<string> items = new List<string>();
            for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
            {
                if (!frameAtt.DashOnly || GraphicsManager.Actions[ii].IsDash)
                {
                    if (ii == (int)member)
                        chosenIndex = items.Count;
                    items.Add(GraphicsManager.Actions[ii].Name);
                }
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = Math.Min(Math.Max(0, chosenIndex), items.Count - 1);
            control.Children.Add(cbValue);
        }


        public override Int32 SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);

            ComboBox cbValue = (ComboBox)control.Children[controlIndex];
            if (!frameAtt.DashOnly)
                return cbValue.SelectedIndex;
            else
            {
                int currentDashValue = -1;
                for (int ii = 0; ii < GraphicsManager.Actions.Count; ii++)
                {
                    if (GraphicsManager.Actions[ii].IsDash)
                    {
                        currentDashValue++;
                        if (currentDashValue == cbValue.SelectedIndex)
                        {
                            return ii;
                        }
                    }
                }
            }
            return 0;
        }

        public override string GetString(Int32 obj, Type type, object[] attributes)
        {
            if (obj >= 0 && obj < GraphicsManager.Actions.Count)
                return GraphicsManager.Actions[obj].Name;
            return "**EMPTY**";
        }
    }
}
