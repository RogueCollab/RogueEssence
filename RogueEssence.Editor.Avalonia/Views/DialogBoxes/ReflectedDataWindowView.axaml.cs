using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RogueEssence.Dev.ViewModels;

// NOTE: The ReflectedDataWindowView is only used for the CategorySpawnEditor so far. This probably needs to be adjusted further for nested saving to work if implemented
namespace RogueEssence.Dev.Views
{
    public partial class ReflectedDataWindowView: ChromelessWindow
    {
        public StackPanel ControlPanel { get; }
        
        public ReflectedDataWindowView()
        {
            this.InitializeComponent();
            ControlPanel = stkContent;
        }
        
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is not ReflectedDataWindowViewModel vm) return;
            

            if (vm.OnLoadAction != null)
                vm.OnLoadAction(ControlPanel);
        }
        

        public void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ReflectedDataWindowViewModel vm) return;
            vm.OnOKAction?.Invoke(ControlPanel);
            this.Close(true);
        }


        public void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(false);
        }
        
        // public void SetViewOnly()
        // {
        //     Button button = this.FindControl<Button>("btnOK");
        //     button.IsEnabled = false;
        // }
    }
}
