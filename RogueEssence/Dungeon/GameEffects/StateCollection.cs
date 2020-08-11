using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Drawing;
using System.Text;
using RogueEssence.Dev;
#if EDITORS
using System.Windows.Forms;
#endif

namespace RogueEssence.Dungeon
{
    [Serializable]
    public abstract class GameplayState : EditorData
    {
        public T Clone<T>() where T : GameplayState { return (T)Clone(); }
        public abstract GameplayState Clone();
    }

    [Serializable]
    public class StateCollection<T> : Dev.EditorData, IEnumerable<T> where T : GameplayState
    {
        [NonSerialized]
        private Dictionary<string, T> pointers;


        public StateCollection()
        {
            pointers = new Dictionary<string, T>();
        }
        protected StateCollection(StateCollection<T> other) : this()
        {
            foreach (string key in other.pointers.Keys)
                pointers[key] = (T)other.pointers[key].Clone();
        }
        public StateCollection<T> Clone() { return new StateCollection<T>(this); }

        public void Clear()
        {
            pointers.Clear();
        }

        public bool Contains<K>() where K : T
        {
            Type type = typeof(K);
            return Contains(type);
        }

        public bool Contains(Type type)
        {
            return pointers.ContainsKey(type.FullName);
        }
        public bool Contains(string typeFullName)
        {
            return pointers.ContainsKey(typeFullName);
        }

        public K Get<K>() where K : T
        {
            Type type = typeof(K);
            T state;
            if (pointers.TryGetValue(type.FullName, out state))
                return (K)state;
            return default(K);
        }

        public T Get(Type type)
        {
            T state;
            if (pointers.TryGetValue(type.FullName, out state))
                return state;
            return default(T);
        }

        public void Set(T state)
        {
            pointers[state.GetType().FullName] = state;
        }

        public void Remove<K>() where K : T
        {
            Type type = typeof(K);
            Remove(type);
        }

        public void Remove(Type type)
        {
            pointers.Remove(type.FullName);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() { return pointers.Values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return pointers.Values.GetEnumerator(); }


        public override string ToString()
        {
            if (pointers.Count == 0)
                return "[Empty " + typeof(T).ToString() + "s]";
            StringBuilder builder = new StringBuilder();
            int count = 0;
            int total = pointers.Count;
            foreach (T value in pointers.Values)
            {
                if (count == 3)
                {
                    builder.Append("...");
                    break;
                }
                builder.Append(value.ToString());
                count++;
                if (count < total)
                    builder.Append(", ");
            }
            return builder.ToString();
        }

#if EDITORS
        protected override void LoadClassControls(TableLayoutPanel control)
        {
            CollectionBox lbxValue = new CollectionBox();
            lbxValue.Dock = DockStyle.Fill;
            lbxValue.Size = new Size(0, 150);
            List<T> states = new List<T>();
            foreach (T state in pointers.Values)
                states.Add(state);
            lbxValue.LoadFromList(typeof(List<T>), states);
            control.Controls.Add(lbxValue);

            Type elementType = typeof(T);
            //add lambda expression for editing a single element
            lbxValue.OnEditItem = (int index, object element, CollectionBox.EditElementOp op) =>
            {
                ElementForm frmData = new ElementForm();
                if (element == null)
                    frmData.Text = "New " + elementType.Name;
                else
                    frmData.Text = element.ToString();

                staticLoadMemberControl(frmData.ControlPanel, "(StateCollection) [" + index + "]", elementType, new object[0] { }, element, true);

                frmData.OnOK += (object okSender, EventArgs okE) =>
                {
                    staticSaveMemberControl(frmData.ControlPanel, "StateCollection", elementType, new object[0] { }, ref element, true);

                    bool itemExists = false;
                    for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
                    {
                        if (ii != index)
                        {
                            if (lbxValue.Collection[ii].GetType() == element.GetType())
                                itemExists = true;
                        }
                    }

                    if (itemExists)
                        MessageBox.Show("Cannot add duplicate states.", "Entry already exists.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        op(index, element);
                        frmData.Close();
                    }
                };
                frmData.OnCancel += (object okSender, EventArgs okE) =>
                {
                    frmData.Close();
                };

                frmData.Show();
            };
        }


        protected override void SaveClassControls(TableLayoutPanel control)
        {
            CollectionBox lbxValue = (CollectionBox)control.Controls[0];

            pointers = new Dictionary<string, T>();
            for (int ii = 0; ii < lbxValue.Collection.Count; ii++)
                pointers[lbxValue.Collection[ii].GetType().FullName] = (T)lbxValue.Collection[ii];
        }

#endif

        private List<T> serializationObjects;

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            serializationObjects = new List<T>();
            foreach(string key in pointers.Keys)
                serializationObjects.Add(pointers[key]);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            pointers = new Dictionary<string, T>();
            for (int ii = 0; ii < serializationObjects.Count; ii++)
                pointers[serializationObjects[ii].GetType().FullName] = serializationObjects[ii];
        }
    }
}
