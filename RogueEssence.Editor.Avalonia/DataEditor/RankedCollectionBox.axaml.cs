using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.Markup.Xaml.Templates;

namespace RogueEssence.Dev.Views
{
    public class RankedCollectionBox : UserControl
    {
        public RankedCollectionBox()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        //TODO: there has to be some way to set the ItemTemplate's text binding in code-behind...
        //public void SetConv(IValueConverter conv)
        //{

        //    ListBox lbx = this.FindControl<ListBox>("lbxItems");
        //    //var template = (DataTemplate)lbx.ItemTemplate;
        //    //var content = template.Content;
        //    var subject = lbx.GetBindingSubject(ListBox.ItemTemplateProperty);
        //    //BindingBase bind = (BindingBase)subject.ToBinding();
        //    //bind.Converter = conv;

        //    Console.WriteLine(subject.ToString());
        //}

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

            ViewModels.CollectionBoxViewModel viewModel = (ViewModels.CollectionBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }
    }
}
