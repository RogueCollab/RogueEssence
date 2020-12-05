using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev
{
    public interface IEditorConverter
    {
        Type GetConvertingType();

        void LoadClassControls(object obj, StackPanel control);

        void LoadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow);

        void SaveClassControls(object obj, StackPanel control);

        void SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow);
    }
}
