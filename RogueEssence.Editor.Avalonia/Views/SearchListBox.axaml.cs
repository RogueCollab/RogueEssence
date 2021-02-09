using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

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

        public void lbxItems_DoubleClick(object sender, RoutedEventArgs e)
        {
            ViewModels.SearchListBoxViewModel viewModel = (ViewModels.SearchListBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxItems_DoubleClick(sender, e);
        }
    }
}
