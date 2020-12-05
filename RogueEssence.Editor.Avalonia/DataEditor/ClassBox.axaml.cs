using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace RogueEssence.Dev.Views
{
    public class ClassBox : UserControl
    {
        public object Object { get; private set; }

        public delegate void EditElementOp(object element);
        public delegate void ElementOp(object element, EditElementOp op);

        public ElementOp OnEditItem;

        public ClassBox()
        {
            this.InitializeComponent();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void LoadFromSource(object source)
        {
            Object = source;
            TextBlock lblName = this.FindControl<TextBlock>("lblName");
            lblName.Text = Object.ToString();
        }


        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            object element = Object;
            OnEditItem(element, LoadFromSource);
        }
    }
}
