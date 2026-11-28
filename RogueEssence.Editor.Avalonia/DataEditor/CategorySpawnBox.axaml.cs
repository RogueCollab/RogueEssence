using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueElements;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Avalonia.Input;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views
{
    public partial class CategorySpawnBox : UserControl
    {
        
        public CategorySpawnBox()
        {
            this.InitializeComponent();
            Button addCategoryButton = this.FindControl<Button>("CategorySpawnBoxAddCategoryButton");
            addCategoryButton.AddHandler(PointerReleasedEvent, CategorySpawnBoxAddCategoryButton_OnPointerReleased, RoutingStrategies.Tunnel);
            Button addItemButtom = this.FindControl<Button>("CategorySpawnBoxAddItemButton");
            addItemButtom.AddHandler(PointerReleasedEvent, CategorySpawnBoxAddItemButton_OnPointerReleased, RoutingStrategies.Tunnel);
        }
        
        bool doubleclick;
        public void doubleClickStart(object sender, RoutedEventArgs e)
        {
            doubleclick = true;
        }

        public void gridCollection_DoubleClick(object sender, PointerReleasedEventArgs e)
        {
            if (!doubleclick)
                return;
            doubleclick = false;

            ViewModels.CategorySpawnBoxViewModel viewModel = (ViewModels.CategorySpawnBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.gridCollection_DoubleClick(sender, e);
        }

        private void CategorySpawnBoxAddCategoryButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            CategorySpawnBoxViewModel vm = (CategorySpawnBoxViewModel) DataContext;
            vm.btnAddCategory_Click(advancedEdit);
        }

        private void CategorySpawnBoxAddItemButton_OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            KeyModifiers modifiers = e.KeyModifiers;
            bool advancedEdit = modifiers.HasFlag(KeyModifiers.Shift);
            CategorySpawnBoxViewModel vm = (CategorySpawnBoxViewModel) DataContext;
            vm.btnAddItem_Click(advancedEdit);
        }
    }
}
