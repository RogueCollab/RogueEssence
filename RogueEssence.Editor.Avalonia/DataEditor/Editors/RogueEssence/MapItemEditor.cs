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
    public class MapItemEditor : Editor<MapItem>
    {
        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, MapItem member, Type[] subGroupStack)
        {
            Avalonia.Controls.Grid innerPanel1 = getSharedRowPanel(2);

            TextBlock lblItem = new TextBlock();
            lblItem.Text = "Item:";
            lblItem.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblItem.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

            ComboBox cbItem = new ComboBox();

            cbItem.VirtualizationMode = ItemVirtualizationMode.Simple;

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[DataManager.DataType.Item];
            List<string> itemKeys = nameIndex.GetOrderedKeys(false);
            int chosenItem = itemKeys.IndexOf(member.Value);

            List<string> items = new List<string>();
            {
                items.Add("---: Money");
                chosenItem++;
            }

            for (int ii = 0; ii < itemKeys.Count; ii++)
                items.Add(itemKeys[ii] + ": " + nameIndex.Entries[itemKeys[ii]].GetLocalString(false));

            var itemsSubject = new Subject<List<string>>();

            cbItem.Bind(ComboBox.ItemsProperty, itemsSubject);
            itemsSubject.OnNext(items);

            innerPanel1.ColumnDefinitions[0].Width = new GridLength(70);
            lblItem.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
            innerPanel1.Children.Add(lblItem);
            cbItem.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
            innerPanel1.Children.Add(cbItem);
            control.Children.Add(innerPanel1);


            StackPanel groupBoxPanel = new StackPanel();
            control.Children.Add(groupBoxPanel);

            populateStack(groupBoxPanel, member);

            cbItem.SelectedIndex = Math.Min(Math.Max(0, chosenItem), items.Count - 1);

            cbItem.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                if (cbItem.SelectedIndex == 0)
                    populateStack(groupBoxPanel, MapItem.CreateMoney(1));
                else
                    populateStack(groupBoxPanel, new MapItem(items[cbItem.SelectedIndex]));
            };
        }

        private void populateStack(StackPanel groupBoxPanel, MapItem member)
        {
            {
                Avalonia.Controls.Grid innerPanel = getSharedRowPanel(2);

                TextBlock lblAmount = new TextBlock();
                lblAmount.Text = "Amount:";
                lblAmount.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblAmount.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                NumericUpDown nudAmount = new NumericUpDown();
                nudAmount.Margin = new Thickness(4, 0, 0, 0);
                nudAmount.Minimum = 1;
                nudAmount.Maximum = Int32.MaxValue;
                nudAmount.Value = member.Amount;

                innerPanel.ColumnDefinitions[0].Width = new GridLength(70);
                lblAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                innerPanel.Children.Add(lblAmount);
                nudAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                innerPanel.Children.Add(nudAmount);
                groupBoxPanel.Children.Add(innerPanel);
            }

            if (member.IsMoney)
                return;

            {
                Avalonia.Controls.Grid innerPanel = getSharedRowPanel(2);

                TextBlock lblAmount = new TextBlock();
                lblAmount.Text = "Amount:";
                lblAmount.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblAmount.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                NumericUpDown nudAmount = new NumericUpDown();
                nudAmount.Margin = new Thickness(4, 0, 0, 0);
                nudAmount.Minimum = 1;
                nudAmount.Maximum = Int32.MaxValue;
                nudAmount.Value = member.Amount;

                innerPanel.ColumnDefinitions[0].Width = new GridLength(70);
                lblAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                innerPanel.Children.Add(lblAmount);
                nudAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                innerPanel.Children.Add(nudAmount);
                groupBoxPanel.Children.Add(innerPanel);
            }

            {
                Avalonia.Controls.Grid innerPanel = getSharedRowPanel(2);

                TextBlock lblAmount = new TextBlock();
                lblAmount.Text = "Hidden Value:";
                lblAmount.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblAmount.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                TextBox txtValue = new TextBox();
                txtValue.Margin = new Thickness(4, 0, 0, 0);
                txtValue.Text = member.HiddenValue;

                innerPanel.ColumnDefinitions[0].Width = new GridLength(70);
                lblAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                innerPanel.Children.Add(lblAmount);
                txtValue.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                innerPanel.Children.Add(txtValue);
                groupBoxPanel.Children.Add(innerPanel);
            }

            CheckBox chkCursed = new CheckBox();
            chkCursed.Margin = new Thickness(0, 4, 0, 0);
            chkCursed.Content = "Cursed";
            chkCursed.IsChecked = member.Cursed;
            groupBoxPanel.Children.Add(chkCursed);

        }


        public override MapItem SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            MapItem result = new MapItem();

            int controlIndex = 0;

            Avalonia.Controls.Grid innerControl1 = (Avalonia.Controls.Grid)control.Children[controlIndex];
            ComboBox cbItem = (ComboBox)innerControl1.Children[1];

            List<string> itemKeys = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetOrderedKeys(false);
            itemKeys.Insert(0, "");

            if (cbItem.SelectedIndex > 0)
                result.Value = itemKeys[cbItem.SelectedIndex];
            else
                result.IsMoney = true;

            controlIndex++;

            // read from the adjustable panel from here on
            StackPanel adjustablePanel = (StackPanel)control.Children[controlIndex];
            controlIndex = 0;
            Avalonia.Controls.Grid innerControl2 = (Avalonia.Controls.Grid)adjustablePanel.Children[controlIndex];
            NumericUpDown nudAmount = (NumericUpDown)innerControl2.Children[1];
            result.Amount = (int)nudAmount.Value;

            if (!result.IsMoney)
            {
                controlIndex++;
                Avalonia.Controls.Grid innerControl3 = (Avalonia.Controls.Grid)adjustablePanel.Children[controlIndex];
                TextBox txtHidden = (TextBox)innerControl3.Children[1];
                result.HiddenValue = txtHidden.Text;

                controlIndex++;
                CheckBox chkCursed = (CheckBox)adjustablePanel.Children[controlIndex];
                result.Cursed = chkCursed.IsChecked.HasValue && chkCursed.IsChecked.Value;
            }

            return result;
        }

        public override string GetString(MapItem obj, Type type, object[] attributes)
        {
            if (obj.IsMoney)
                return String.Format("{0}P", obj.Amount);
            else if (!String.IsNullOrEmpty(obj.Value))
            {
                ItemData entry = DataManager.Instance.GetItem(obj.Value);
                if (entry.MaxStack > 1)
                    return (obj.Cursed ? "[X]" : "") + entry.Name.ToLocal() + " (" + obj.Amount + ")";
                else
                    return (obj.Cursed ? "[X]" : "") + entry.Name.ToLocal();
            }
            return "---";
        }
    }
}
