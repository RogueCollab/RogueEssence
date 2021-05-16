using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dev.Views;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;

namespace RogueEssence.Dev
{
    public class StringConv
    {
        public Type ObjectType;
        public IEditor Editor;
        public object[] Attributes;

        public StringConv()
        {
            ObjectType = typeof(object);
            Editor = new ObjectEditor();
            Attributes = new object[0];
        }
        public StringConv(Type type, IEditor editor, object[] attributes)
        {
            ObjectType = type;
            Editor = editor;
            Attributes = attributes;
        }

        public string GetString(object obj)
        {
            return Editor.GetString(obj, ObjectType, Attributes);
        }
    }
}
