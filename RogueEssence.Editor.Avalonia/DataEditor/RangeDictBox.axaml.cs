﻿using Avalonia;
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
    public partial class RangeDictBox : UserControl
    {
        public RangeDictBox()
        {
            this.InitializeComponent();
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

            ViewModels.RangeDictBoxViewModel viewModel = (ViewModels.RangeDictBoxViewModel)DataContext;
            if (viewModel == null)
                return;
            viewModel.lbxCollection_DoubleClick(sender, e);
        }

        public void nudStart_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.RangeDictBoxViewModel viewModel = (ViewModels.RangeDictBoxViewModel)DataContext;
            viewModel.AdjustOtherLimit((int)e.NewValue, false);
        }

        public void nudEnd_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            ViewModels.RangeDictBoxViewModel viewModel = (ViewModels.RangeDictBoxViewModel)DataContext;
            viewModel.AdjustOtherLimit((int)e.NewValue, true);
        }

        public void SetListContextMenu(ContextMenu menu)
        {
            DataGrid lbx = this.FindControl<DataGrid>("gridItems");
            lbx.ContextMenu = menu;
        }
    }
}
