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
using System.Threading.Tasks;

namespace RogueEssence.Dev.Views
{
    public class DataEditForm : ParentForm
    {
        public delegate Task<bool> OKEvent();
        public OKEvent SelectedOKEvent;

        public bool Cancel;
        //public event Action SelectedCancelEvent;

        public StackPanel ControlPanel { get; }

        public DataEditForm()
        {
            InitializeComponent();

            ControlPanel = this.FindControl<StackPanel>("stkContent");
            
#if DEBUG
            this.AttachDevTools();
#endif
        }

        protected virtual void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async Task SaveChildren()
        {
            for (int ii = children.Count - 1; ii >= 0; ii--)
            {
                DataEditForm dataEditor = children[ii] as DataEditForm;
                if (dataEditor != null)
                    await dataEditor.SaveChildren();
            }
            if (SelectedOKEvent != null)
                await SelectedOKEvent.Invoke();
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

        public virtual async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Design.IsDesignMode)
                return;
            
            CloseChildren();
            
            if(!Cancel)
                await SaveChildren();
        }

        public async void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool close = false;
            if (SelectedOKEvent != null)
                close = await SelectedOKEvent.Invoke();
            if (close)
                Close();
        }

        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //SelectedCancelEvent?.Invoke();
            Cancel = true;
            Close();
        }

        public void SetViewOnly()
        {
            Button button = this.FindControl<Button>("btnOK");
            button.IsEnabled = false;
        }

    }
}
