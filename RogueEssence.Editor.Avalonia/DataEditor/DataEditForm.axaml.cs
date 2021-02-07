using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using RogueEssence;
using RogueEssence.Dev;
using Microsoft.Xna.Framework;
using Avalonia.Threading;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace RogueEssence.Dev.Views
{
    public class DataEditForm : ParentForm
    {
        public event Action SelectedOKEvent;
        public event Action SelectedCancelEvent;

        public StackPanel ControlPanel { get; }

        public DataEditForm()
        {
            InitializeComponent();

            ControlPanel = this.FindControl<StackPanel>("stkContent");
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        //TODO: this is a workaround to a bug in text wrapping
        //the window size must be modified in order to invalidate a cached value for width
        //remove this once the bug is fixed
        public void Window_Loaded(object sender, EventArgs e)
        {
            if (Design.IsDesignMode)
                return;
            this.Width = this.Width + 10;
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Design.IsDesignMode)
                return;

            CloseChildren();
        }

        public void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SelectedOKEvent?.Invoke();
        }

        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedCancelEvent?.Invoke();
        }

    }
}
