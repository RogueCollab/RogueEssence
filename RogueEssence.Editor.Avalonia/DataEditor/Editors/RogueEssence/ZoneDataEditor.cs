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
using System.Diagnostics;
using System.IO;

namespace RogueEssence.Dev
{
    public class ZoneDataEditor : Editor<ZoneData>
    {

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, ZoneData obj, Type[] subGroupStack)
        {
            base.LoadWindowControls(control, parent, parentType, name, type, attributes, obj, subGroupStack);

            LoadLabelControl(control, "Script Events", "Events that can be created in the lua script for this zone.");

            Button btnTest = new Button();
            btnTest.Margin = new Thickness(0, 4, 0, 0);
            btnTest.Content = "Open Script Folder";
            btnTest.Click += async (object sender, RoutedEventArgs e) =>
            {
                string zonescriptdir = LuaEngine.MakeZoneScriptPath(parent, "");

                if (!Directory.Exists(zonescriptdir))
                {
                    await MessageBox.Show(control.GetOwningForm(), String.Format("This zone has not been saved under the current mod-under-edit.  Please switch to the desired mod and save it first."), "Invalid Operation", MessageBox.MessageBoxButtons.Ok);
                }
                else
                {
                    try
                    {
                        if (OperatingSystem.IsWindows())
                            Process.Start("explorer.exe", zonescriptdir);
                        else if (OperatingSystem.IsLinux())
                            Process.Start("xdg-open", zonescriptdir);
                        else if (OperatingSystem.IsMacOS())
                            Process.Start("open", zonescriptdir);
                        else
                            throw new NotSupportedException("File open not supported on current system.");
                    }
                    catch (Exception ex)
                    {
                        DiagManager.Instance.LogError(ex);
                    }
                }
            };
            control.Children.Add(btnTest);

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
