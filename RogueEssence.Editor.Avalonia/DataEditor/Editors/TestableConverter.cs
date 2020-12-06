using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RogueEssence.Dev.Views;

namespace RogueEssence.Dev
{
    public abstract class TestableConverter<T> : EditorConverter<T>
    {
        public override void LoadWindowControls(T obj, StackPanel control)
        {
            base.LoadWindowControls(obj, control);

            Button btnTest = new Button();
            btnTest.Margin = new Avalonia.Thickness(0, 4, 0, 0);
            btnTest.Content = "Test";
            btnTest.Click += (object sender, RoutedEventArgs e) =>
            {
                lock (GameBase.lockObj)
                    btnTest_Click(sender, e, obj);
            };
            control.Children.Add(btnTest);
        }




        protected abstract void btnTest_Click(object sender, RoutedEventArgs e, T obj);
    }
}
