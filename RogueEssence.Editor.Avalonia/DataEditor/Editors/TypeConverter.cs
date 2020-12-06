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

namespace RogueEssence.Dev
{
    public class TypeConverter : EditorConverter<Type>
    {
        public override void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, Type member, bool isWindow)
        {
            TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
            Type baseType = dataAtt.BaseClass;

            Type[] children = baseType.GetAssignableTypes();

            Avalonia.Controls.Grid sharedRowPanel = DataEditor.getSharedRowPanel(2);

            TextBlock lblType = new TextBlock();
            lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblType.Text = name + ":";
            sharedRowPanel.Children.Add(lblType);
            sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
            lblType.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            ComboBox cbValue = new ComboBox();
            cbValue.Margin = new Thickness(4, 0, 0, 0);
            sharedRowPanel.Children.Add(cbValue);
            cbValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            List<string> items = new List<string>();
            int selection = 0;
            for (int ii = 0; ii < children.Length; ii++)
            {
                Type childType = children[ii];
                items.Add(childType.GetDisplayName());

                if (childType == (Type)member)
                    selection = ii;
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = selection;

            control.Children.Add(sharedRowPanel);

        }


        public override void SaveClassControls(StackPanel control, string name, Type type, object[] attributes, ref Type member, bool isWindow)
        {
            int controlIndex = 0;
            TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
            Type baseType = dataAtt.BaseClass;

            Type[] children = baseType.GetAssignableTypes();

            Avalonia.Controls.Grid subGrid = (Avalonia.Controls.Grid)control.Children[controlIndex];
            ComboBox cbValue = (ComboBox)subGrid.Children[1];
            member = children[cbValue.SelectedIndex];
            controlIndex++;
        }
    }
}
