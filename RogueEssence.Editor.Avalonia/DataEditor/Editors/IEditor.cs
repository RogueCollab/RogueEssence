﻿using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev
{
    public interface IEditor
    {
        Type GetAttributeType();
        Type GetConvertingType();

        void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow);

        void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, object member);

        void LoadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow);

        object SaveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow);

        object SaveWindowControls(StackPanel control, string name, Type type, object[] attributes);

        object SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow);
    }
}
