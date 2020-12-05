using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev
{
    public abstract class EditorConverter<T> : IEditorConverter
    {
        public Type GetConvertingType() { return typeof(T); }

        public virtual void LoadClassControls(T obj, StackPanel control)
        {
            DataEditor.StaticLoadClassControls(obj, control);
        }

        public virtual void LoadMemberControl(T obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            DataEditor.StaticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }

        public virtual void SaveClassControls(T obj, StackPanel control)
        {
            DataEditor.StaticSaveClassControls(obj, control);
        }

        public virtual void SaveMemberControl(T obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            DataEditor.StaticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        void IEditorConverter.LoadClassControls(object obj, StackPanel control)
        {
            LoadClassControls((T)obj, control);
        }
        void IEditorConverter.LoadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            LoadMemberControl((T)obj, control, name, type, attributes, member, isWindow);
        }
        void IEditorConverter.SaveClassControls(object obj, StackPanel control)
        {
            SaveClassControls((T)obj, control);
        }
        void IEditorConverter.SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            SaveMemberControl((T)obj, control, name, type, attributes, ref member, isWindow);
        }
    }
}
