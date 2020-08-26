using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;

namespace RogueEssence.Dev
{
    public class SpawnListConverter : EditorConverter<ISpawnList>
    {
        //    public override void LoadClassControls(SpawnList<object> obj, TableLayoutPanel control)
        //    {
        //        //go through all members and add for them
        //        //control starts off clean; this is the control that will have all member controls on it
        //        try
        //        {
        //            //create panel and place in rect
        //            Panel main_panel = new Panel();
        //            main_panel.AutoScroll = true;
        //            main_panel.Location = rect.Location;
        //            main_panel.Size = rect.Size;
        //            //set panel's tag to a new list
        //            List<(object, int)> tag_list = new List<(object, int)>();
        //            for(int ii = 0; ii < obj.Count; ii++)
        //                tag_list.Add((obj.GetSpawn(ii), obj.GetSpawnRate(ii)));
        //            //populate list with tuples created from tuples in the current list
        //            main_panel.Tag = tag_list;
        //            control.Controls.Add(main_panel);
        //            Button add_button = new Button();

        //            Type elementType = obj.GetType().GetGenericArguments()[0];
        //            for (int ii = 0; ii < tag_list.Count; ii++)
        //            {
        //                AddPanel(main_panel, add_button, tag_list, ii);
        //            }
        //            //add "add" button
        //            add_button.Text = "Add";
        //            add_button.Location = new Point(0, 70 * tag_list.Count);
        //            add_button.Size = new Size(50, 23);
        //            //clicking "add" creates an element form to choose with
        //            add_button.Click += (object sender, EventArgs e) =>
        //            {
        //                Dev.ElementForm frmData = new Dev.ElementForm();
        //                frmData.Text = "Edit Element";
        //                int index = tag_list.Count;
        //                Rectangle boxRect = new Rectangle(new Point(), GetMemberControlSize(elementType, null, null));
        //                LoadMemberControl(frmData.ControlPanel, "(SpawnRate) " /*+ name*/ + "[" + index + "]", elementType, null, null, boxRect);
        //                frmData.ResizeForPanelSize(boxRect.Size);

        //                //if selected OK, adds object to list, inserts new panel, and loads it
        //                if (frmData.ShowDialog() == DialogResult.OK)
        //                {
        //                    int controlIndex = 0;
        //                    object spawn = null;
        //                    SaveMemberControl(frmData.ControlPanel, elementType, null, ref spawn, ref controlIndex);
        //                    tag_list.Add((spawn, 10));
        //                    AddPanel(main_panel, add_button, tag_list, tag_list.Count - 1);
        //                    add_button.Location = new Point(0, tag_list.Count * 70);
        //                    //then modify chances of all the other panels
        //                    UpdateTotalChance(main_panel, tag_list);
        //                }
        //            };
        //            main_panel.Controls.Add(add_button);
        //            UpdateTotalChance(main_panel, tag_list);
        //        }
        //        catch (Exception e)
        //        {
        //            DiagManager.Instance.LogError(e);
        //        }
        //    }

        //    public override void SaveClassControls(IStateCollection obj, TableLayoutPanel control)
        //    {
        //        try
        //        {
        //            //set list to panel tag
        //            Panel panel = (Panel)control.Controls[0];
        //            spawns = (List<SpawnRate>)panel.Tag;
        //            //update spawn total
        //            spawnTotal = ListTotal(spawns);
        //        }
        //        catch (Exception e)
        //        {
        //            DiagManager.Instance.LogError(e);
        //        }
        //    }

        //    void AddPanel(Panel main_panel, Button add_button, List<(object, int)> tag_list, int ii)
        //    {
        //        //add panel to main panel
        //        Panel panel = new Panel();
        //        panel.Location = new Point(0, ii * 70);
        //        panel.Size = new Size(main_panel.Size.Width - 18, 70);

        //        (object, int) tuple = tag_list[ii];

        //        //set up name and calculated percent
        //        Label spawn_name = new Label();
        //        spawn_name.Location = new System.Drawing.Point(0, 5);
        //        spawn_name.AutoEllipsis = true;
        //        spawn_name.Size = new Size(panel.Size.Width - 60, 13);
        //        spawn_name.Text = tuple.Item1.ToString();

        //        Label spawn_weight = new Label();
        //        spawn_weight.Location = new System.Drawing.Point(panel.Size.Width - 60, 53);
        //        spawn_weight.AutoSize = true;
        //        spawn_weight.Text = "Weight: " + tuple.Item2;

        //        Label spawn_chance = new Label();
        //        spawn_chance.Location = new System.Drawing.Point(panel.Size.Width - 50, 5);
        //        spawn_chance.AutoSize = true;
        //        spawn_chance.Text = "%";
        //        spawn_chance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        //        //set up buttons:
        //        Button edit_button = new Button();
        //        edit_button.Text = "Edit";
        //        edit_button.Location = new Point(0, 48);
        //        edit_button.Size = new Size(34, 21);
        //        //clicking edit opens the element form
        //        edit_button.Click += (object sender, EventArgs e) =>
        //        {
        //            Dev.ElementForm frmData = new Dev.ElementForm();
        //            frmData.Text = "Edit Element";
        //            int index = tag_list.IndexOf(tuple);
        //            Rectangle boxRect = new Rectangle(new Point(), GetMemberControlSize(typeof(T), null, tuple.Item1));
        //            LoadMemberControl(frmData.ControlPanel, "(SpawnRate) " /*+ name*/ + "[" + index + "]", typeof(T), null, tuple.Item1, boxRect);
        //            frmData.ResizeForPanelSize(boxRect.Size);
        //            //if selected OK, sets object in list, modifies label name
        //            if (frmData.ShowDialog() == DialogResult.OK)
        //            {
        //                int controlIndex = 0;
        //                object spawn = tuple.Item1;
        //                SaveMemberControl(frmData.ControlPanel, typeof(T), null, ref spawn, ref controlIndex);
        //                tuple.Item1 = (T)spawn;
        //            }
        //        };

        //        Button delete_button = new Button();
        //        delete_button.Text = "Delete";
        //        delete_button.Location = new Point(34, 48);
        //        delete_button.Size = new Size(34, 21);
        //        //clicking delete removes the object, and removes panel and shifts all controls after this one up
        //        delete_button.Click += (object sender, EventArgs e) =>
        //        {
        //            int index = tag_list.IndexOf(tuple);
        //            tag_list.RemoveAt(index);
        //            main_panel.Controls.RemoveAt(index);
        //            for (int jj = index; jj < tag_list.Count; jj++)
        //            {
        //                Panel shift_panel = (Panel)main_panel.Controls[jj];
        //                shift_panel.Location = new Point(0, jj * 70);
        //            }
        //            UpdateTotalChance(main_panel, tag_list);
        //            add_button.Location = new Point(0, tag_list.Count * 70);
        //        };

        //        //set up trackbar to spawn rate
        //        //when trackbar changes, set appearance rate for all panels
        //        TrackBar trackBar = new TrackBar();
        //        trackBar.LargeChange = 10;
        //        trackBar.Location = new System.Drawing.Point(0, 20);
        //        trackBar.Maximum = 100;
        //        trackBar.Minimum = 1;
        //        trackBar.Size = new System.Drawing.Size(panel.Size.Width, 45);
        //        trackBar.TickFrequency = 10;
        //        trackBar.Value = tuple.Item2;
        //        trackBar.ValueChanged += (object sender, EventArgs e) =>
        //        {
        //            spawnTotal = spawnTotal - tuple.Item2 + trackBar.Value;//MUST UPDATE THE SPAWNTOTAL TO STAY IN SYNC
        //            tuple.Item2 = trackBar.Value;
        //            spawn_weight.Text = "Weight: " + trackBar.Value;
        //            UpdateTotalChance(main_panel, tag_list);
        //        };

        //        panel.Controls.Add(delete_button);
        //        panel.Controls.Add(edit_button);
        //        panel.Controls.Add(spawn_weight);
        //        panel.Controls.Add(spawn_chance);
        //        panel.Controls.Add(spawn_name);
        //        panel.Controls.Add(trackBar);
        //        main_panel.Controls.Add(panel);
        //        main_panel.Controls.SetChildIndex(panel, ii);
        //    }

        //    void UpdateTotalChance(Panel main_panel, List<(object, int)> tag_list)
        //    {
        //        int total = ListTotal(tag_list);
        //        for (int jj = 0; jj < tag_list.Count; jj++)
        //        {
        //            Label label = (Label)main_panel.Controls[jj].Controls[3];
        //            label.Text = ((decimal)tag_list[jj].Item2 / total).ToString("P2");
        //        }
        //    }

        //    int ListTotal(List<(object, int)> tag_list)
        //    {
        //        int total = 0;
        //        foreach (var tuple in tag_list)
        //            total += tuple.Item2;
        //        return total;
        //    }
    }
}
