using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev
{
    public abstract class EditorConverter<T> : IEditorConverter
    {
        public Type GetConvertingType() { return typeof(T); }

        public virtual void LoadClassControls(T obj, TableLayoutPanel control)
        {
            DataEditor.StaticLoadClassControls(obj, control);
        }

        public virtual void LoadMemberControl(T obj, TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            DataEditor.StaticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }

        public virtual void SaveClassControls(T obj, TableLayoutPanel control)
        {
            DataEditor.StaticSaveClassControls(obj, control);
        }

        public virtual void SaveMemberControl(T obj, TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            DataEditor.StaticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        void IEditorConverter.LoadClassControls(object obj, TableLayoutPanel control)
        {
            LoadClassControls((T)obj, control);
        }
        void IEditorConverter.LoadMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            LoadMemberControl((T)obj, control, name, type, attributes, member, isWindow);
        }
        void IEditorConverter.SaveClassControls(object obj, TableLayoutPanel control)
        {
            SaveClassControls((T)obj, control);
        }
        void IEditorConverter.SaveMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            SaveMemberControl((T)obj, control, name, type, attributes, ref member, isWindow);
        }
    }
}
