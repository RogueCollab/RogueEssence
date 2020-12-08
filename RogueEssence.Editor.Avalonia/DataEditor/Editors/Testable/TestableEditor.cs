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
    public abstract class TestableEditor<T> : Editor<T>
    {

        public override void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, T obj)
        {
            base.LoadWindowControls(control, name, type, attributes, obj);

            Button btnTest = new Button();
            btnTest.Margin = new Avalonia.Thickness(0, 4, 0, 0);
            btnTest.Content = "Test";
            btnTest.Click += (object sender, RoutedEventArgs e) =>
            {
                lock (GameBase.lockObj)
                {
                    if (CheckTest())
                    {
                        T testObj = (T)DataEditor.SaveWindowControls(control, "", type, attributes);
                        RunTest(testObj);
                    }
                }
            };
            control.Children.Add(btnTest);
        }


        private bool CheckTest()
        {
            return DungeonScene.Instance.ActiveTeam.Players.Count > 0 && DungeonScene.Instance.FocusedCharacter != null;
        }

        protected abstract void RunTest(T data);
    }
}
