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
        public object[] Attributes;

        public StringConv()
        {
            ObjectType = typeof(object);
            Attributes = new object[0];
        }
        public StringConv(Type type, object[] attributes)
        {
            ObjectType = type;
            Attributes = attributes;
        }

        public string GetString(object obj)
        {
            return DataEditor.GetString(obj, ObjectType, Attributes);
        }
    }
}
