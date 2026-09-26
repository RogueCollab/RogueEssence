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
using System.Transactions;

namespace RogueEssence.Dev.Views
{
    public partial class DataEditForm : ParentForm
    {
        public delegate Task<bool> OKEvent();
        public OKEvent SelectedOKEvent;

        //public event Action SelectedCancelEvent;

        public StackPanel ControlPanel { get; }

        public DataEditForm()
        {
            InitializeComponent();

            ControlPanel = this.FindControl<StackPanel>("stkContent");
        }
        
        public async Task SaveChildren()
        {
            for (int ii = children.Count - 1; ii >= 0; ii--)
            {
                DataEditForm dataEditor = children[ii] as DataEditForm;
                if (dataEditor != null)
                {
                    await dataEditor.SaveChildren();
                    if (dataEditor.SelectedOKEvent != null)
                        await dataEditor.SelectedOKEvent.Invoke();
                }
            }
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

        public virtual async void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            if (Design.IsDesignMode)
                return;
            
            if (!Cancel)
            {
                if (!OK && children.Count > 0)
                {
                    //X button was clicked when there are children, cancel the close, popup the children, and add a warning message.
                    e.Cancel = true;
                    FocusChildren();
                    Task<MessageBox.MessageBoxResult> task =  MessageBox.Show(this, "Are you sure you want to close all subwindows?  Your changes will not be saved.", "Confirm Close",
                        MessageBox.MessageBoxButtons.YesNo);
                    MessageBox.MessageBoxResult result = await task;
                    if (result == MessageBox.MessageBoxResult.Yes)
                    {
                        Cancel = true;
                        Close();
                        return;
                    }
                }
            }
            
            if (!e.Cancel)
                CloseChildren();
        }

        public async void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool close = false;
            if (SelectedOKEvent != null)
            {
                await SaveChildren();
                close = await SelectedOKEvent.Invoke();
            }
            if (close)
            {
                OK = true;
                Close();
            }
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
