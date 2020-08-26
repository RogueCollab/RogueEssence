using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;

namespace RogueEssence.Dev
{
    public abstract class TestableConverter<T> : EditorConverter<T>
    {
        public override void LoadClassControls(T obj, TableLayoutPanel control)
        {
            int initialHeight = control.Height;
            base.LoadClassControls(obj, control);

            int totalHeight = control.Height - initialHeight;

            Button btnTest = new Button();
            btnTest.Name = "btnTest";
            btnTest.Dock = DockStyle.Fill;
            btnTest.Size = new System.Drawing.Size(0, 29);
            btnTest.TabIndex = 0;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += (object sender, EventArgs e) => { btnTest_Click(sender, e, obj); };
            control.Controls.Add(btnTest);
        }




        protected abstract void btnTest_Click(object sender, EventArgs e, T obj);
    }
}
