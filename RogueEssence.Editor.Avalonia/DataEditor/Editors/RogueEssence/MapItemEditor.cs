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
            {
                Avalonia.Controls.Grid innerPanel1 = getSharedRowPanel(2);

                TextBlock lblItem = new TextBlock();
                lblItem.Text = "Item:";
                lblItem.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblItem.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                ComboBox cbItem = new ComboBox();

                cbItem.VirtualizationMode = ItemVirtualizationMode.Simple;
                int chosenItem = member.IsMoney ? -1 : member.Value;
                EntryDataIndex nameIndex = DataManager.Instance.DataIndices[DataManager.DataType.Item];

                List<string> items = new List<string>();
                {
                    items.Add("---: Money");
                    chosenItem++;
                }

                for (int ii = 0; ii < nameIndex.Count; ii++)
                    items.Add(ii.ToString() + ": " + nameIndex.Entries[ii.ToString()].GetLocalString(false));

                var itemsSubject = new Subject<List<string>>();

                cbItem.Bind(ComboBox.ItemsProperty, itemsSubject);
                itemsSubject.OnNext(items);

                innerPanel1.ColumnDefinitions[0].Width = new GridLength(70);
                lblItem.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                innerPanel1.Children.Add(lblItem);
                cbItem.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                innerPanel1.Children.Add(cbItem);
                control.Children.Add(innerPanel1);




                Avalonia.Controls.Grid innerPanel2 = getSharedRowPanel(2);

                TextBlock lblAmount = new TextBlock();
                lblAmount.Text = "Amount:";
                lblAmount.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblAmount.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                NumericUpDown nudHiddenVal = new NumericUpDown();
                nudHiddenVal.Margin = new Thickness(4, 0, 0, 0);
                nudHiddenVal.Minimum = Int32.MinValue;
                nudHiddenVal.Maximum = Int32.MaxValue;
                nudHiddenVal.Value = member.HiddenValue;

                innerPanel2.ColumnDefinitions[0].Width = new GridLength(70);
                lblAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                innerPanel2.Children.Add(lblAmount);
                nudHiddenVal.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                innerPanel2.Children.Add(nudHiddenVal);
                control.Children.Add(innerPanel2);

                CheckBox chkCursed = new CheckBox();
                chkCursed.Margin = new Thickness(0, 4, 0, 0);
                chkCursed.Content = "Cursed";
                chkCursed.IsChecked = member.Cursed;
                control.Children.Add(chkCursed);



                cbItem.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    if (cbItem.SelectedIndex == 0)
                    {
                        lblAmount.Text = "Amount:";
                        nudHiddenVal.Minimum = 1;
                        chkCursed.IsVisible = false;
                    }
                    else
                    {
                        lblAmount.Text = "Hidden Val:";
                        nudHiddenVal.Minimum = Int32.MinValue;
                        chkCursed.IsVisible = true;
                    }
                };
                cbItem.SelectedIndex = Math.Min(Math.Max(0, chosenItem), items.Count - 1);

            }

        }

        public override MapItem SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            MapItem result = new MapItem();

            int controlIndex = 0;

            Avalonia.Controls.Grid innerControl1 = (Avalonia.Controls.Grid)control.Children[controlIndex];

            int innerControlIndex = 0;
            innerControlIndex++;
            ComboBox cbItem = (ComboBox)innerControl1.Children[innerControlIndex];
            if (cbItem.SelectedIndex > 0)
                result.Value = cbItem.SelectedIndex - 1;
            else
                result.IsMoney = true;

            controlIndex++;
            Avalonia.Controls.Grid innerControl2 = (Avalonia.Controls.Grid)control.Children[controlIndex];
            innerControlIndex = 0;
            innerControlIndex++;
            NumericUpDown nudHiddenVal = (NumericUpDown)innerControl2.Children[innerControlIndex];
            result.HiddenValue = (int)nudHiddenVal.Value;

            controlIndex++;
            CheckBox chkCursed = (CheckBox)control.Children[controlIndex];
            if (!result.IsMoney)
                result.Cursed = chkCursed.IsChecked.HasValue && chkCursed.IsChecked.Value;

            return result;
        }

        public override string GetString(MapItem obj, Type type, object[] attributes)
        {
            if (obj.IsMoney)
                return String.Format("{0}P", obj.HiddenValue);
            else if (obj.Value > -1)
            {
                ItemData entry = DataManager.Instance.GetItem(obj.Value);
                if (entry.MaxStack > 1)
                    return (obj.Cursed ? "[X]" : "") + entry.Name.ToLocal() + " (" + obj.HiddenValue + ")";
                else
                    return (obj.Cursed ? "[X]" : "") + entry.Name.ToLocal();
            }
            return "---";
        }
    }
}
