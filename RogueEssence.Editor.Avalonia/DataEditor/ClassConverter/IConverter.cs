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
    public interface IClassConverter
    {
        Type GetConvertingType();
        string GetClassString(object obj);
    }
}
