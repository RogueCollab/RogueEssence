using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.VisualTree;
using Avalonia.Input;

namespace RogueEssence.Dev.Views
{
    public class RangeDictBox : UserControl
    {
        public RangeDictBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        bool doubleclick;
        public void doubleClickStart(object sender, RoutedEventArgs e)
        {
            doubleclick = true;
        }

        public void lbxCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            if (!doubleclick)
                return;
            doubleclick = false;

            ViewModels.DictionaryBoxViewModel viewModel = (ViewModels.DictionaryBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }

    }
}
