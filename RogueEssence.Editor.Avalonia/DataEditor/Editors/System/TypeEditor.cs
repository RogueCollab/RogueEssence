﻿using System;
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
    public class TypeEditor : Editor<Type>
    {
        public override bool DefaultSubgroup => true;
        public override bool DefaultDecoration => false;
        public override bool DefaultLabel => false;
        public override bool DefaultType => true;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, Type member, Type[] subGroupStack)
        {
            TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
            Type baseType = dataAtt.BaseClass;

            Type[] children = baseType.GetAssignableTypes();
            control.DataContext = children;

            Avalonia.Controls.Grid sharedRowPanel = getSharedRowPanel(2);

            TextBlock lblType = new TextBlock();
            lblType.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblType.Text = name + ":";
            sharedRowPanel.Children.Add(lblType);
            sharedRowPanel.ColumnDefinitions[0].Width = new GridLength(30);
            lblType.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);

            ComboBox cbValue = new SearchComboBox();
            cbValue.Margin = new Thickness(4, 0, 0, 0);
            sharedRowPanel.Children.Add(cbValue);
            cbValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);

            List<string> items = new List<string>();
            int selection = 0;
            for (int ii = 0; ii < children.Length; ii++)
            {
                Type childType = children[ii];
                items.Add(childType.GetFriendlyTypeString());

                if (childType == (Type)member)
                    selection = ii;
            }

            var subject = new Subject<List<string>>();
            cbValue.Bind(ComboBox.ItemsSourceProperty, subject);
            subject.OnNext(items);
            cbValue.SelectedIndex = selection;

            control.Children.Add(sharedRowPanel);

        }


        public override Type SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;
            TypeConstraintAttribute dataAtt = ReflectionExt.FindAttribute<TypeConstraintAttribute>(attributes);
            Type baseType = dataAtt.BaseClass;

            Type[] children = (Type[])control.DataContext;

            Avalonia.Controls.Grid subGrid = (Avalonia.Controls.Grid)control.Children[controlIndex];
            ComboBox cbValue = (ComboBox)subGrid.Children[1];
            return children[cbValue.SelectedIndex];
        }
    }
}
