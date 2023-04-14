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
    public class InvItemEditor : Editor<InvItem>
    {
        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, InvItem member, Type[] subGroupStack)
        {
            Avalonia.Controls.Grid innerPanel1 = getSharedRowPanel(2);

            TextBlock lblItem = new TextBlock();
            lblItem.Text = "Item:";
            lblItem.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            lblItem.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

            ComboBox cbItem = new SearchComboBox();

            cbItem.VirtualizationMode = ItemVirtualizationMode.Simple;

            EntryDataIndex nameIndex = DataManager.Instance.DataIndices[DataManager.DataType.Item];
            List<string> itemKeys = nameIndex.GetOrderedKeys(false);
            int chosenItem = itemKeys.IndexOf(member.ID);

            List<string> items = new List<string>();

            for (int ii = 0; ii < itemKeys.Count; ii++)
                items.Add(itemKeys[ii] + ": " + nameIndex.Get(itemKeys[ii]).GetLocalString(false));

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

            cbItem.SelectedIndex = Math.Min(Math.Max(0, chosenItem), items.Count - 1);

            cbItem.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                populateStack(groupBoxPanel, new InvItem(itemKeys[cbItem.SelectedIndex]));
            };

            //invalid item ID, default to the selected one
            if (itemKeys[cbItem.SelectedIndex] != member.ID)
                populateStack(groupBoxPanel, new InvItem(itemKeys[cbItem.SelectedIndex]));
            else
                populateStack(groupBoxPanel, member);

        }

        private bool canShowAmount(InvItem member)
        {
            bool showAmount = false;
            ItemData summary = DataManager.Instance.GetItem(member.ID);
            if (summary.MaxStack > 1)
                showAmount = true;
            return showAmount;
        }

        private void populateStack(StackPanel groupBoxPanel, InvItem member)
        {
            groupBoxPanel.Children.Clear();

            bool showAmount = canShowAmount(member);
            if (showAmount)
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
            else
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


        public override InvItem SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            InvItem result = new InvItem();

            int controlIndex = 0;

            Avalonia.Controls.Grid innerControl1 = (Avalonia.Controls.Grid)control.Children[controlIndex];
            ComboBox cbItem = (ComboBox)innerControl1.Children[1];

            List<string> itemKeys = DataManager.Instance.DataIndices[DataManager.DataType.Item].GetOrderedKeys(false);

            result.ID = itemKeys[cbItem.SelectedIndex];
            controlIndex++;

            // read from the adjustable panel from here on
            StackPanel adjustablePanel = (StackPanel)control.Children[controlIndex];
            controlIndex = 0;

            bool showAmount = canShowAmount(result);
            if (showAmount)
            {
                Avalonia.Controls.Grid innerControl2 = (Avalonia.Controls.Grid)adjustablePanel.Children[controlIndex];
                NumericUpDown nudAmount = (NumericUpDown)innerControl2.Children[1];
                result.Amount = (int)nudAmount.Value;
                controlIndex++;
            }
            else
            {
                Avalonia.Controls.Grid innerControl3 = (Avalonia.Controls.Grid)adjustablePanel.Children[controlIndex];
                TextBox txtHidden = (TextBox)innerControl3.Children[1];
                result.HiddenValue = txtHidden.Text;
                controlIndex++;
            }

            CheckBox chkCursed = (CheckBox)adjustablePanel.Children[controlIndex];
            result.Cursed = chkCursed.IsChecked.HasValue && chkCursed.IsChecked.Value;

            return result;
        }

        public override string GetString(InvItem obj, Type type, object[] attributes)
        {
            if (!String.IsNullOrEmpty(obj.ID))
            {
                string nameStr = "";
                if (obj.Price > 0)
                    nameStr += String.Format("${0} ", obj.Price);
                if (obj.Cursed)
                    nameStr += "[X]";

                ItemData entry = DataManager.Instance.GetItem(obj.ID);

                nameStr += entry.Name.ToLocal();
                if (entry.MaxStack > 1)
                    nameStr += String.Format("({0})", obj.Amount);

                if (!String.IsNullOrEmpty(obj.HiddenValue))
                    nameStr += String.Format("[{0}]", obj.HiddenValue);

                return nameStr;
            }
            return "---";
        }
    }
}
