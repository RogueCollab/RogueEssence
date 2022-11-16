using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;
using RogueEssence.Dungeon;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Collections;
using Avalonia.Styling;

namespace RogueEssence.Dev.Views
{
    public class SearchComboBox : ComboBox, IStyleable
    {
        Type IStyleable.StyleKey => typeof(ComboBox);

        private string workingSearch;
        private bool[] processedKey;
        public SearchComboBox() : base()
        {
            workingSearch = "";
            processedKey = new bool[26];
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                int idx = e.Key - Key.A;
                if (!processedKey[idx])
                {
                    processedKey[idx] = true;
                    char letter = (char)(idx + 'A');
                    workingSearch = workingSearch + letter.ToString();
                    int letterIndex = 0;
                    foreach (string obj in Items)
                    {
                        if (obj.StartsWith(workingSearch, StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.ScrollIntoView(letterIndex);
                            break;
                        }
                        letterIndex++;
                    }
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                int idx = e.Key - Key.A;
                processedKey[idx] = false;
            }
            base.OnKeyUp(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            workingSearch = "";
            base.OnPointerMoved(e);
        }
    }
}
