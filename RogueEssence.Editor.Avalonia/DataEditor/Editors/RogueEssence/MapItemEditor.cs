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




                Avalonia.Controls.Grid innerPanel2 = getSharedRowPanel(2);

                TextBlock lblAmount = new TextBlock();
                lblAmount.Text = "Amount:";
                lblAmount.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                lblAmount.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                NumericUpDown nudAmount = new NumericUpDown();
                nudAmount.Margin = new Thickness(4, 0, 0, 0);
                nudAmount.Minimum = Int32.MinValue;
                nudAmount.Maximum = Int32.MaxValue;
                nudAmount.Value = member.Amount;

                innerPanel2.ColumnDefinitions[0].Width = new GridLength(70);
                lblAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 0);
                innerPanel2.Children.Add(lblAmount);
                nudAmount.SetValue(Avalonia.Controls.Grid.ColumnProperty, 1);
                innerPanel2.Children.Add(nudAmount);
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
                        nudAmount.Minimum = 1;
                        chkCursed.IsVisible = false;
                    }
                    else
                    {
                        lblAmount.Text = "Hidden Val:";
                        nudAmount.Minimum = Int32.MinValue;
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

            List<string> itemKeys = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetOrderedKeys(false);
            itemKeys.Insert(0, "");

            if (cbItem.SelectedIndex > 0)
                result.Value = itemKeys[cbItem.SelectedIndex];
            else
                result.IsMoney = true;

            controlIndex++;
            Avalonia.Controls.Grid innerControl2 = (Avalonia.Controls.Grid)control.Children[controlIndex];
            innerControlIndex = 0;
            innerControlIndex++;
            NumericUpDown nudAmount = (NumericUpDown)innerControl2.Children[innerControlIndex];
            result.Amount = (int)nudAmount.Value;

            controlIndex++;
            CheckBox chkCursed = (CheckBox)control.Children[controlIndex];
            if (!result.IsMoney)
                result.Cursed = chkCursed.IsChecked.HasValue && chkCursed.IsChecked.Value;

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
