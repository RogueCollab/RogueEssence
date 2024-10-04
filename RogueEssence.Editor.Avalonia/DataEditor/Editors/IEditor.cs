using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev
{
    public interface IEditor
    {
        bool SimpleEditor { get; }
        Type GetAttributeType();
        Type GetConvertingType();

        void LoadClassControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, object member, bool isWindow, Type[] subGroupStack, bool advancedEdit);

        void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, object member, Type[] subGroupStack);

        void LoadMemberControl(string parent, object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow, Type[] subGroupStack);

        object SaveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow, Type[] subGroupStack, bool advancedEdit);

        object SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack);

        object SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow, Type[] subGroupStack);

        string GetString(object obj, Type type, object[] attributes);

        string GetTypeString();
    }
}
