using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using Avalonia.Input;

namespace RogueEssence.Dev.Views
{
    public class SearchListBox : UserControl
    {
        public SearchListBox()
        {
            this.InitializeComponent();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void slb_DataContextChanged(object sender, EventArgs e)
        {
            ViewModels.SearchListBoxViewModel viewModel = (ViewModels.SearchListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            TextBox textBox = this.FindControl<TextBox>("txtSearch");
            //TODO: memory leak?
            textBox.GetObservable(TextBox.TextProperty).Subscribe(viewModel.txtSearch_TextChanged);
        }

        bool doubleclick;
        public void doubleClickStart(object sender, RoutedEventArgs e)
        {
            doubleclick = true;
        }

        public void lbxItems_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            if (!doubleclick)
                return;
            doubleclick = false;

            ViewModels.SearchListBoxViewModel viewModel = (ViewModels.SearchListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxItems_DoubleClick(sender, e);
        }
    }
}
