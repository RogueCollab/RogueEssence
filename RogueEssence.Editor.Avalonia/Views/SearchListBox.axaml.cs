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
            TextBox textBox = this.FindControl<TextBox>("txtSearch");
            textBox.GetObservable(TextBox.TextProperty).Subscribe(viewModel.txtSearch_TextChanged);
        }


        public void lbxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModels.SearchListBoxViewModel viewModel = (ViewModels.SearchListBoxViewModel)DataContext;
            viewModel.lbxItems_SelectionChanged(sender, e);
        }

        public void lbxItems_DoubleClick(object sender, RoutedEventArgs e)
        {
            ViewModels.SearchListBoxViewModel viewModel = (ViewModels.SearchListBoxViewModel)DataContext;
            viewModel.lbxItems_DoubleClick(sender, e);
        }
    }
}
