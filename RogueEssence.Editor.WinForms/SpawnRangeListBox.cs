using System;
using System.Collections;
using System.Windows.Forms;
using RogueElements;

namespace RogueEssence.Dev
{
    public partial class SpawnRangeListBox : UserControl
    {
        private bool dragging;
        public ISpawnRangeList Collection { get; private set; }

        public int SelectedIndex
        {
            get { return lbxCollection.SelectedIndex; }
            set { lbxCollection.SelectedIndex = value; }
        }

        public delegate void EditElementOp(int index, object element);
        public delegate void ElementOp(int index, object element, EditElementOp op);

        public ElementOp OnEditItem;
        public ReflectionExt.TypeStringConv StringConv;

        public SpawnRangeListBox()
        {
            InitializeComponent();
            StringConv = DefaultStringConv;
        }

        private string DefaultStringConv(object obj)
        {
            return obj.ToString();
        }

        public void LoadFromList(Type type, ISpawnRangeList source)
        {
            Collection = (ISpawnRangeList)Activator.CreateInstance(type);
            for (int ii = 0; ii < source.Count; ii++)
            {
                object obj = source.GetSpawn(ii);
                int rate = source.GetSpawnRate(ii);
                IntRange range = source.GetSpawnRange(ii);
                Collection.Add(obj, range, rate);
            }
            for (int ii = 0; ii < source.Count; ii++)
                lbxCollection.Items.Add(getSpawnString(ii));
        }

        private void editItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count);
            Collection.SetSpawn(index, element);
            lbxCollection.Items[index] = getSpawnString(index);
        }

        private void insertItem(int index, object element)
        {
            index = Math.Min(Math.Max(0, index), Collection.Count+1);
            Collection.Insert(index, element, new IntRange(0), 10);
            lbxCollection.Items.Insert(index, getSpawnString(index));
        }

        private void lbxCollection_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lbxCollection.IndexFromPoint(e.X, e.Y);
            if (index > -1)
            {
                object element = Collection.GetSpawn(index);
                OnEditItem(index, element, editItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int index = lbxCollection.SelectedIndex;
            if (index < 0)
                index = lbxCollection.Items.Count;
            object element = null;
            OnEditItem(index, element, insertItem);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1)
            {
                Collection.RemoveAt(lbxCollection.SelectedIndex);
                lbxCollection.Items.RemoveAt(lbxCollection.SelectedIndex);
            }
        }

        private void Switch(int a, int b)
        {
            object obj = Collection.GetSpawn(a);
            int rate = Collection.GetSpawnRate(a);
            Collection.SetSpawn(a, Collection.GetSpawn(b));
            Collection.SetSpawnRate(a, Collection.GetSpawnRate(b));
            Collection.SetSpawn(b, obj);
            Collection.SetSpawnRate(b, rate);
            updateSpawnString(a);
            updateSpawnString(b);
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > 0)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex - 1);
                lbxCollection.SelectedIndex = index - 1;
            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lbxCollection.SelectedIndex > -1 && lbxCollection.SelectedIndex < lbxCollection.Items.Count - 1)
            {
                int index = lbxCollection.SelectedIndex;
                Switch(lbxCollection.SelectedIndex, lbxCollection.SelectedIndex + 1);
                lbxCollection.SelectedIndex = index + 1;
            }
        }

        private string getSpawnString(int index)
        {
            object obj = Collection.GetSpawn(index);
            int rate = Collection.GetSpawnRate(index);
            IntRange range = Collection.GetSpawnRange(index);
            return String.Format("{2}-{3} [{1}] {0}", StringConv(obj), rate, range.Min, range.Max-1);
        }

        private void spawnRateTrackBar_Scroll(object sender, EventArgs e)
        {
            lblWeight.Text = String.Format("Weight:\n{0}", spawnRateTrackBar.Value);

            if (!dragging)
            {
                int index = lbxCollection.SelectedIndex;
                Collection.SetSpawnRate(index, spawnRateTrackBar.Value);
                updateSpawnString(index);
                spawnRateTrackBar.Focus();
            }
        }

        private void spawnRateTrackBar_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
        }

        private void spawnRateTrackBar_MouseCaptureChanged(object sender, EventArgs e)
        {
            dragging = false;
            int index = lbxCollection.SelectedIndex;
            if (index > -1)
            {
                Collection.SetSpawnRate(index, spawnRateTrackBar.Value);
                updateSpawnString(index);
                spawnRateTrackBar.Focus();
            }
        }

        bool updating;
        private void updateSpawnString(int index)
        {
            updating = true;
            lbxCollection.Items[index] = getSpawnString(index);
            updating = false;
        }

        private void lbxCollection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updating)
                return;
            int index = lbxCollection.SelectedIndex;
            if (index > -1)
            {
                spawnRateTrackBar.Value = Collection.GetSpawnRate(index);
                spawnRateTrackBar.Enabled = true;
                IntRange range = Collection.GetSpawnRange(index);
                nudStart.Value = range.Min;
                nudEnd.Value = range.Max-1;
                nudStart.Enabled = true;
                nudEnd.Enabled = true;
                lblWeight.Text = String.Format("Weight:\n{0}", spawnRateTrackBar.Value);
            }
            else
            {
                spawnRateTrackBar.Value = 1;
                spawnRateTrackBar.Enabled = false;
                nudStart.Value = 0;
                nudEnd.Value = 1;
                nudStart.Enabled = false;
                nudEnd.Enabled = false;
                lblWeight.Text = String.Format("Weight:\n---");
            }
        }

        private void nudStart_ValueChanged(object sender, EventArgs e)
        {
            if (nudEnd.Value < nudStart.Value)
                nudEnd.Value = nudStart.Value;

            int index = lbxCollection.SelectedIndex;
            if (index > -1)
            {
                Collection.SetSpawnRange(index, new IntRange((int)nudStart.Value, (int)nudEnd.Value+1));
                updateSpawnString(index);
            }
        }

        private void nudEnd_ValueChanged(object sender, EventArgs e)
        {
            if (nudEnd.Value < nudStart.Value)
                nudStart.Value = nudEnd.Value;

            int index = lbxCollection.SelectedIndex;
            if (index > -1)
            {
                Collection.SetSpawnRange(index, new IntRange((int)nudStart.Value, (int)nudEnd.Value + 1));
                updateSpawnString(index);
            }
        }
    }
}
