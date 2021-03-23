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
    public abstract class ClassConverter<T> : IClassConverter
    {
        public Type GetConvertingType() { return typeof(T); }

        public abstract string GetClassString(T obj);

        string IClassConverter.GetClassString(object obj)
        {
            return GetClassString((T)obj);
        }
    }
}
