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
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public class RankedCollectionBox : UserControl
    {
        public RankedCollectionBox()
        {
            this.InitializeComponent();
            Button button = this.FindControl<Button>("RankedCollectionBoxAddButton");
            button.AddHandler(PointerReleasedEvent, RankedCollectionBoxAddButton_OnPointerReleased, RoutingStrategies.Tunnel);
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

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }

        private void RankedCollectionBoxAddButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            CollectionBoxViewModel vm = (CollectionBoxViewModel) DataContext;
            vm.btnAdd_Click(advancedEdit);
        }
    }
}
