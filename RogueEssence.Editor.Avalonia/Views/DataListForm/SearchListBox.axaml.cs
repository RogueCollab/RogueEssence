using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace RogueEssence.Dev.Views
{
    public class SearchListBox : UserControl
    {
        public SearchListBox()
        {
            this.InitializeComponent();

            TextBox textBox = this.FindControl<TextBox>("txtSearch");
            textBox.GetObservable(TextBox.TextProperty).Subscribe(txtSearch_TextChanged);
        }

        private void txtSearch_TextChanged(string text)
        {

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
