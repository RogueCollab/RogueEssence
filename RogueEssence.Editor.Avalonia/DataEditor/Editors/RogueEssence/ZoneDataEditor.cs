using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.Views;
using RogueEssence.Script;
using Avalonia;

namespace RogueEssence.Dev
{
    public class ZoneDataEditor : Editor<ZoneData>
    {

        public override void LoadWindowControls(StackPanel control, string parent, string name, Type type, object[] attributes, ZoneData obj, Type[] subGroupStack)
        {
            base.LoadWindowControls(control, parent, name, type, attributes, obj, subGroupStack);

            LoadLabelControl(control, "Script Events", "Events that can be created in the lua script for this zone.");

            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = Avalonia.Media.Brushes.LightGray;
            border.Margin = new Thickness(2);
            control.Children.Add(border);

            StackPanel groupBoxPanel = new StackPanel();
            groupBoxPanel.Margin = new Thickness(2);
            border.Child = groupBoxPanel;

            for (int ii = 0; ii < (int)LuaEngine.EZoneCallbacks.Invalid; ii++)
            {
                LuaEngine.EZoneCallbacks ev = (LuaEngine.EZoneCallbacks)ii;
                TextBlock lblName = new TextBlock();
                lblName.Margin = new Thickness(0, 4, 0, 0);
                lblName.Text = ev.ToString();
                groupBoxPanel.Children.Add(lblName);
            }
        }
    }
}
