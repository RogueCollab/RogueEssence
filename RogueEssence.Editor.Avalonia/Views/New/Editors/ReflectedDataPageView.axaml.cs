using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Data;
using RogueEssence.Dev.ViewModels;

namespace RogueEssence.Dev.Views;

public interface ISaveable
{
    Task<bool> Save();
}

public partial class ReflectedDataPageView : UserControl, ISaveable
{
    public StackPanel ControlPanel { get; }

    
    public ReflectedDataPageView()
    {
        InitializeComponent();
        ControlPanel = stkContent;
    }
    
    public async Task<bool> Save()
    {
        if (DataContext is ReflectedDataPageViewModel vm && vm.OnOKAction != null)
        {
            return await vm.OnOKAction(ControlPanel);
            
        }
        return true;
    }
    public async void btnApply_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReflectedDataPageViewModel vm)
            await vm.ApplySave();
    }
    
    public async void btnOK_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReflectedDataPageViewModel vm)
        {
            bool close = await vm.ApplySave();
            if (close)
                vm.Close();
        }
    }
    
    public void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ReflectedDataPageViewModel vm)
            vm.Close();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DataContextChanged += OnDataContextChanged;
        OnDataContextChanged(this, EventArgs.Empty);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DataContextChanged -= OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not EditorPageViewModel vm) return;
    
        vm.AttachedView = this;
        Console.WriteLine($"View set on {vm}");

        if (DataContext is ReflectedDataPageViewModel reflectedVm && reflectedVm.OnLoadAction != null)
            reflectedVm.OnLoadAction(ControlPanel);
    }
    
    // public virtual async void Window_Closing(object sender, WindowClosingEventArgs e)
    // {
    //     if (Design.IsDesignMode)
    //         return;
    //         
    //     if (!Cancel)
    //     {
    //         if (!OK && children.Count > 0)
    //         {
    //             //X button was clicked when there are children, cancel the close, popup the children, and add a warning message.
    //             e.Cancel = true;
    //             FocusChildren();
    //             Task<MessageBox.MessageBoxResult> task =  MessageBox.Show(this, "Are you sure you want to close all subwindows?  Your changes will not be saved.", "Confirm Close",
    //                 MessageBox.MessageBoxButtons.YesNo);
    //             MessageBox.MessageBoxResult result = await task;
    //             if (result == MessageBox.MessageBoxResult.Yes)
    //             {
    //                 Cancel = true;
    //                 Close();
    //                 return;
    //             }
    //         }
    //     }
    //         
    //     if (!e.Cancel)
    //         CloseChildren();
    // }
    //
    // public async void btnOK_Click(object sender, RoutedEventArgs e)
    // {
    //     bool close = false;
    //     if (SelectedOKEvent != null)
    //     {
    //         await SaveChildren();
    //         close = await SelectedOKEvent.Invoke();
    //     }
    //     if (close)
    //     {
    //         OK = true;
    //         Close();
    //     }
    // }
    //
    // public void btnCancel_Click(object sender, RoutedEventArgs e)
    // {
    //     //SelectedCancelEvent?.Invoke();
    //     Cancel = true;
    //     Close();
    // }
    //
    // public void SetViewOnly()
    // {
    //     Button button = this.FindControl<Button>("btnOK");
    //     button.IsEnabled = false;
    // }
}